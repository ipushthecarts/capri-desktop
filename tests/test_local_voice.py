import contextlib
import http.client
import importlib.util
import io
import json
from pathlib import Path
import sys
import tempfile
import threading
import types
import unittest
from unittest.mock import patch
import wave

spec = importlib.util.spec_from_file_location('local_server', Path(__file__).parents[1] / 'local-voice/server.py')
server = importlib.util.module_from_spec(spec)
spec.loader.exec_module(server)


def sample_wav():
    out = io.BytesIO()
    with wave.open(out, 'wb') as wav:
        wav.setnchannels(1)
        wav.setsampwidth(2)
        wav.setframerate(24000)
        wav.writeframes(b'\x00\x00\x10\x00')
    return out.getvalue()


class FakeEngine:
    model = None

    def __init__(self):
        self.calls = []
        self.error = None

    def render(self, text, voice):
        self.calls.append((text, voice))
        if self.error:
            raise self.error
        return sample_wav()


class ProtocolTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.httpd = server.ThreadingHTTPServer(('127.0.0.1', 0), server.Handler)
        cls.httpd.daemon_threads = True
        cls.thread = threading.Thread(target=cls.httpd.serve_forever, daemon=True)
        cls.thread.start()

    @classmethod
    def tearDownClass(cls):
        cls.httpd.shutdown()
        cls.httpd.server_close()
        cls.thread.join()

    def setUp(self):
        self.httpd.engine = FakeEngine()

    def request(self, data=None, method='POST', path='/v1/audio/speech', headers=None, raw=None):
        body = json.dumps(data).encode() if raw is None and data is not None else raw
        connection = http.client.HTTPConnection('127.0.0.1', self.httpd.server_port, timeout=3)
        try:
            connection.request(method, path, body, headers or {'Content-Type': 'application/json'})
            response = connection.getresponse()
            return response.status, response.read(), response.getheader('Content-Type')
        finally:
            connection.close()

    def test_health_does_not_load_model(self):
        status, body, _ = self.request(method='GET', path='/health')
        self.assertEqual(status, 200)
        self.assertFalse(json.loads(body)['model_loaded'])
        self.assertEqual(self.httpd.engine.calls, [])

    def test_speech_response_is_pcm_wav(self):
        status, body, mime = self.request({'input': ' Hello, choom! ', 'voice': 'alba'})
        self.assertEqual((status, mime), (200, 'audio/wav'))
        with wave.open(io.BytesIO(body)) as wav:
            self.assertEqual((wav.getnchannels(), wav.getsampwidth(), wav.getframerate()), (1, 2, 24000))
        self.assertEqual(self.httpd.engine.calls, [('Hello, choom!', 'alba')])

    def test_reference_file_with_spaces(self):
        with tempfile.TemporaryDirectory() as tmp:
            sample = Path(tmp) / 'my voice.wav'
            sample.write_bytes(sample_wav())
            status, _, _ = self.request({'input': 'Hi', 'voice': str(sample)})
            self.assertEqual(status, 200)
            self.assertEqual(self.httpd.engine.calls[-1][1], str(sample.resolve()))

    def test_remote_reference_is_rejected(self):
        for voice in ['https://example.com/voice.wav', 'hf://user/repo/voice.wav', 'relative.wav', 'missing']:
            with self.subTest(voice=voice):
                self.assertEqual(self.request({'input': 'Hi', 'voice': voice})[0], 400)
        self.assertEqual(self.httpd.engine.calls, [])

    def test_bad_input_never_reaches_model(self):
        for data in [[], None, {'input': ''}, {'input': 42}, {'input': 'x' * 4001}, {'input': 'Hi', 'voice': []}]:
            with self.subTest(data=str(data)[:50]):
                self.assertEqual(self.request(raw=json.dumps(data).encode())[0], 400)
        self.assertEqual(self.request(raw=b'{bad')[0], 400)
        self.assertEqual(self.request(raw=b'x' * 33000)[0], 400)
        self.assertEqual(self.httpd.engine.calls, [])

    def test_rejects_browser_and_dns_rebinding(self):
        for extra in [{'Origin': 'https://evil.example'}, {'Sec-Fetch-Site': 'cross-site'}, {'Host': 'evil.example'}]:
            self.assertEqual(self.request({'input': 'Hi'}, headers={'Content-Type': 'application/json', **extra})[0], 403)
        self.assertEqual(self.httpd.engine.calls, [])

    def test_wrong_content_type_or_route(self):
        self.assertEqual(self.request(raw=b'{}', headers={'Content-Type': 'text/plain'})[0], 400)
        self.assertEqual(self.request({'input': 'Hi'}, path='/elsewhere')[0], 404)

    def test_busy_and_inference_failure_are_explicit(self):
        self.httpd.engine.error = BlockingIOError('busy')
        self.assertEqual(self.request({'input': 'Hi'})[0], 503)
        self.httpd.engine.error = RuntimeError('private diagnostic')
        with self.assertLogs(level='ERROR'):
            status, body, _ = self.request({'input': 'Hi'})
        self.assertEqual(status, 500)
        self.assertNotIn(b'private diagnostic', body)


class EngineTests(unittest.TestCase):
    def test_caches_voice_and_converts_float_to_pcm16(self):
        try:
            import numpy as np
        except ImportError:
            self.skipTest('NumPy is installed by Setup-PocketVoice.cmd')
        calls = []

        class Audio:
            def detach(self): return self
            def cpu(self): return self
            def numpy(self): return np.array([-2., -1., 0., 1., 2.])

        class Model:
            sample_rate = 24000
            def get_state_for_audio_prompt(self, voice):
                calls.append(('voice', voice))
                return object()
            def generate_audio(self, state, text): return Audio()

        def load_model(**kwargs):
            calls.append(('load', kwargs))
            return Model()

        fake_torch = types.SimpleNamespace(set_num_threads=lambda n: calls.append(('threads', n)), inference_mode=contextlib.nullcontext)
        fake_pocket = types.SimpleNamespace(TTSModel=types.SimpleNamespace(load_model=load_model))
        engine = server.Engine()
        with patch.dict(sys.modules, {'torch': fake_torch, 'pocket_tts': fake_pocket}):
            for _ in range(2):
                body = engine.render('Hi', 'alba')
            engine.render('Again', 'marius')
        self.assertEqual(sum(c[0] == 'load' for c in calls), 1)
        self.assertEqual([c for c in calls if c[0] == 'voice'], [('voice', 'alba'), ('voice', 'marius')])
        with wave.open(io.BytesIO(body)) as wav:
            pcm = np.frombuffer(wav.readframes(5), dtype='<i2')
            self.assertEqual(pcm.tolist(), [-32767, -32767, 0, 32767, 32767])

    def test_busy_engine_does_not_start_another_job(self):
        engine = server.Engine()
        engine.lock.acquire()
        try:
            with self.assertRaises(BlockingIOError): engine.render('Hi', 'alba')
        finally:
            engine.lock.release()


if __name__ == '__main__':
    unittest.main()
