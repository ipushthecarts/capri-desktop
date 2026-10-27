"""Loopback-only Pocket TTS adapter. No cloud inference or ElevenLabs dependency."""
from __future__ import annotations

import argparse
import io
import json
import logging
import threading
import wave
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

PRESETS = frozenset({'alba', 'marius', 'javert', 'jean', 'fantine', 'cosette', 'eponine', 'azelma'})
MAX_TEXT = 4000
MAX_BODY = 32768


def validate_voice(value: str) -> str:
    if value in PRESETS:
        return value
    path = Path(value).expanduser()
    if not path.is_absolute() or path.suffix.lower() not in {'.wav', '.mp3', '.flac', '.safetensors'} or not path.is_file():
        raise ValueError('Choose a preset or an existing full local path to WAV, MP3, FLAC or safetensors.')
    return str(path.resolve())


class Engine:
    def __init__(self, language='english', threads=2):
        self.language = language
        self.threads = threads
        self.lock = threading.Lock()
        self.model = None
        self.voice_name = None
        self.voice_state = None

    def render(self, text, voice):
        # Reject overlapping jobs instead of accumulating an unbounded model queue.
        if not self.lock.acquire(blocking=False):
            raise BlockingIOError('Pocket TTS is still speaking. Try again shortly.')
        try:
            import numpy as np
            import torch
            from pocket_tts import TTSModel
            if self.model is None:
                logging.info('Loading Pocket TTS on CPU; first run downloads model files.')
                torch.set_num_threads(self.threads)
                self.model = TTSModel.load_model(language=self.language)
            if voice != self.voice_name:
                self.voice_state = self.model.get_state_for_audio_prompt(voice)
                self.voice_name = voice
            with torch.inference_mode():
                audio = self.model.generate_audio(self.voice_state, text)
            pcm = (np.clip(audio.detach().cpu().numpy(), -1.0, 1.0) * 32767).astype('<i2')
            output = io.BytesIO()
            with wave.open(output, 'wb') as wav:
                wav.setnchannels(1)
                wav.setsampwidth(2)
                wav.setframerate(self.model.sample_rate)
                wav.writeframes(pcm.tobytes())
            return output.getvalue()
        finally:
            self.lock.release()


class Handler(BaseHTTPRequestHandler):
    server_version = 'CapriLocalVoice/1.0'

    def allowed(self):
        # No CORS; reject browser-originated requests and DNS-rebinding hosts.
        allowed_hosts = {f'127.0.0.1:{self.server.server_port}', f'localhost:{self.server.server_port}'}
        if self.headers.get('Host', '').lower() not in allowed_hosts or self.headers.get('Origin') or self.headers.get('Sec-Fetch-Site'):
            self.respond(403, b'Local desktop clients only.', 'text/plain')
            return False
        return True

    def respond(self, status, body, content_type='application/json'):
        self.send_response(status)
        self.send_header('Content-Type', content_type)
        self.send_header('Content-Length', str(len(body)))
        self.send_header('Cache-Control', 'no-store')
        self.end_headers()
        try:
            self.wfile.write(body)
        except (BrokenPipeError, ConnectionResetError):
            pass

    def do_GET(self):
        if not self.allowed():
            return
        if self.path != '/health':
            self.respond(404, b'{}')
            return
        self.respond(200, json.dumps({'status': 'ready', 'model_loaded': self.server.engine.model is not None}).encode())

    def do_POST(self):
        if not self.allowed():
            return
        if self.path != '/v1/audio/speech':
            self.respond(404, b'{}')
            return
        try:
            if self.headers.get('Transfer-Encoding'):
                raise ValueError('Transfer-Encoding is unsupported.')
            if self.headers.get_content_type() != 'application/json':
                raise ValueError('Send application/json.')
            length = int(self.headers.get('Content-Length', '0'))
            if not 0 < length <= MAX_BODY:
                raise ValueError('Invalid request size.')
            data = json.loads(self.rfile.read(length))
            if not isinstance(data, dict):
                raise ValueError('Expected a JSON object.')
            text = data.get('input')
            voice = data.get('voice', 'alba')
            if not isinstance(text, str) or not text.strip() or len(text) > MAX_TEXT:
                raise ValueError(f'Provide 1-{MAX_TEXT} characters of text.')
            if not isinstance(voice, str):
                raise ValueError('Voice must be a string.')
            voice = validate_voice(voice)
        except (ValueError, UnicodeError) as error:
            self.respond(400, json.dumps({'error': str(error)}).encode())
            return
        try:
            audio = self.server.engine.render(text.strip(), voice)
        except BlockingIOError as error:
            self.respond(503, json.dumps({'error': str(error)}).encode())
            return
        except Exception:
            logging.exception('Speech generation failed; check model downloads and the selected voice.')
            self.respond(500, b'{"error":"Speech generation failed. Check the Pocket TTS console."}')
            return
        self.respond(200, audio, 'audio/wav')

    def setup(self):
        super().setup()
        self.connection.settimeout(15)

    def log_message(self, fmt, *args):
        # Log status/path only, never spoken text or reference contents.
        logging.info(fmt, *args)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--port', type=int, default=8765)
    parser.add_argument('--threads', type=int, default=2)
    parser.add_argument('--language', default='english')
    args = parser.parse_args()
    if not 1024 <= args.port <= 65535 or not 1 <= args.threads <= 32:
        parser.error('Port must be 1024-65535 and threads 1-32.')
    logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')
    server = ThreadingHTTPServer(('127.0.0.1', args.port), Handler)
    server.daemon_threads = True
    server.engine = Engine(args.language, args.threads)
    logging.info('Listening on http://127.0.0.1:%s. Keep this window open. Ctrl+C stops it.', args.port)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        server.server_close()


if __name__ == '__main__':
    main()
