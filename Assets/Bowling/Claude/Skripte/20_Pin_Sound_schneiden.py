"""Zerlegt eine AIFF-Aufnahme mit mehreren Pin-Anschlägen in einzelne WAV-Dateien.

Aufruf:  python -I split_pins.py <eingabe.aiff> <ausgabeordner> [--write]
Ohne --write werden nur die gefundenen Anschläge ausgegeben.
Nur Standardbibliothek (kein numpy/ffmpeg nötig).
"""
import math
import struct
import sys
import wave
from pathlib import Path


def read_aiff(path):
    data = Path(path).read_bytes()
    assert data[:4] == b"FORM" and data[8:12] in (b"AIFF", b"AIFC"), "keine AIFF-Datei"
    pos, channels, frames, bits, rate, pcm = 12, None, None, None, None, None
    while pos + 8 <= len(data):
        cid, size = data[pos:pos + 4], struct.unpack(">I", data[pos + 4:pos + 8])[0]
        body = data[pos + 8:pos + 8 + size]
        if cid == b"COMM":
            channels, frames, bits = struct.unpack(">hIh", body[:8])
            exp, mant = struct.unpack(">HQ", body[8:18])          # 80-bit IEEE extended
            rate = mant * 2.0 ** ((exp & 0x7FFF) - 16383 - 63)
        elif cid == b"SSND":
            offset = struct.unpack(">I", body[:4])[0]
            pcm = body[8 + offset:]
        pos += 8 + size + (size & 1)
    assert bits == 16, f"nur 16 Bit unterstützt, Datei hat {bits}"
    samples = struct.unpack(f">{frames * channels}h", pcm[:frames * channels * 2])
    return channels, int(round(rate)), samples


def envelope(samples, channels, rate, win_ms=5):
    win = int(rate * win_ms / 1000)
    frames = len(samples) // channels
    env = []
    for start in range(0, frames - win, win):
        acc = 0
        for f in range(start, start + win):
            for c in range(channels):
                s = samples[f * channels + c]
                acc += s * s
        env.append(math.sqrt(acc / (win * channels)))
    return env, win


def find_onsets(env, win, rate, rel_db=-18.0, min_gap_ms=150):
    peak = max(env)
    thresh = peak * 10 ** (rel_db / 20)
    gap = int(min_gap_ms / 1000 * rate / win)
    onsets, last = [], -gap
    for i in range(1, len(env)):
        if env[i] >= thresh and env[i - 1] < thresh and i - last >= gap:
            onsets.append(i)
            last = i
    return onsets, peak


def main():
    src, out_dir = sys.argv[1], Path(sys.argv[2])
    write = "--write" in sys.argv
    channels, rate, samples = read_aiff(src)
    frames = len(samples) // channels
    env, win = envelope(samples, channels, rate)
    onsets, peak = find_onsets(env, win, rate)
    print(f"{src}: {channels} Kanäle, {rate} Hz, {frames / rate:.3f} s, {len(onsets)} Anschläge")

    pre = int(0.005 * rate)            # 5 ms Vorlauf
    max_len = int(0.2 * rate)          # höchstens 0,2 s je Anschlag (Ausklingen auf -40 dB nach ca. 0,15 s)
    fade = int(0.06 * rate)            # 60 ms Ausblenden
    for n, o in enumerate(onsets):
        start = max(0, o * win - pre)
        nxt = onsets[n + 1] * win - pre if n + 1 < len(onsets) else frames
        end = min(nxt, start + max_len, frames)
        seg_peak = max(env[o:min(len(env), o + int(0.05 * rate / win) + 1)])
        print(f"  {n + 1:02d}: {start / rate:6.3f} s – {end / rate:6.3f} s, Spitze {20 * math.log10(seg_peak / peak):5.1f} dB")
        if not write:
            continue
        out = []
        length = end - start
        for i in range(length):
            g = 1.0 if i < length - fade else (length - i) / fade
            for c in range(channels):
                out.append(int(samples[(start + i) * channels + c] * g))
        out_dir.mkdir(parents=True, exist_ok=True)
        with wave.open(str(out_dir / f"PinHit_{n + 1:02d}.wav"), "wb") as w:
            w.setnchannels(channels)
            w.setsampwidth(2)
            w.setframerate(rate)
            w.writeframes(struct.pack(f"<{len(out)}h", *out))


if __name__ == "__main__":
    main()
