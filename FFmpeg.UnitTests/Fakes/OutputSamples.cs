namespace HanumanInstitute.FFmpeg.UnitTests;

public static class OutputSamples
{

    public const string FFmpegInfo1 = @"ffmpeg version N-83507-g8fa18e0 Copyright (c) 2000-2017 the FFmpeg developers
  built with gcc 5.4.0 (GCC)
  configuration: --enable-gpl --enable-version3 --enable-cuda --enable-cuvid --enable-d3d11va --enable-dxva2 --enable-libmfx --enable-nvenc --enable-avisynth --enable-bzlib --enable-fontconfig --enable-frei0r --enable-gnutls --enable-iconv --enable-libass --enable-libbluray --enable-libbs2b --enable-libcaca --enable-libfreetype --enable-libgme --enable-libgsm --enable-libilbc --enable-libmodplug --enable-libmp3lame --enable-libopencore-amrnb --enable-libopencore-amrwb --enable-libopenh264 --enable-libopenjpeg --enable-libopus --enable-librtmp --enable-libsnappy --enable-libsoxr --enable-libspeex --enable-libtheora --enable-libtwolame --enable-libvidstab --enable-libvo-amrwbenc --enable-libvorbis --enable-libvpx --enable-libwavpack --enable-libwebp --enable-libx264 --enable-libx265 --enable-libxavs --enable-libxvid --enable-libzimg --enable-lzma --enable-zlib
  libavutil      55. 47.100 / 55. 47.100
  libavcodec     57. 80.100 / 57. 80.100
  libavformat    57. 66.102 / 57. 66.102
  libavdevice    57.  2.100 / 57.  2.100
  libavfilter     6. 73.100 /  6. 73.100
  libswscale      4.  3.101 /  4.  3.101
  libswresample   2.  4.100 /  2.  4.100
  libpostproc    54.  2.100 / 54.  2.100
Input #0, mp3, from 'E:\DJ Project - Soapte\03 Nu.mp3':
  Metadata:
    title           : Nu
    artist          : DJ Project
    album           : Soapte
    date            : 2005
    comment         : mafiotzii
    track           : 3
    genre           : Dance
  Duration: 00:03:26.11, start: 0.025057, bitrate: 192 kb/s
    Stream #0:0: Audio: mp3, 44100 Hz, stereo, s16p, 192 kb/s
    Metadata:
      encoder         : LAME3.92 
Output #0, mp4, to 'C:\test2.mp4':
";

    public const string FFmpegInfo2 = @"ffmpeg version N-83507-g8fa18e0 Copyright (c) 2000-2017 the FFmpeg developers
  built with gcc 5.4.0 (GCC)
  configuration: --enable-gpl --enable-version3 --enable-cuda --enable-cuvid --enable-d3d11va --enable-dxva2 --enable-libmfx --enable-nvenc --enable-avisynth --enable-bzlib --enable-fontconfig --enable-frei0r --enable-gnutls --enable-iconv --enable-libass --enable-libbluray --enable-libbs2b --enable-libcaca --enable-libfreetype --enable-libgme --enable-libgsm --enable-libilbc --enable-libmodplug --enable-libmp3lame --enable-libopencore-amrnb --enable-libopencore-amrwb --enable-libopenh264 --enable-libopenjpeg --enable-libopus --enable-librtmp --enable-libsnappy --enable-libsoxr --enable-libspeex --enable-libtheora --enable-libtwolame --enable-libvidstab --enable-libvo-amrwbenc --enable-libvorbis --enable-libvpx --enable-libwavpack --enable-libwebp --enable-libx264 --enable-libx265 --enable-libxavs --enable-libxvid --enable-libzimg --enable-lzma --enable-zlib
  libavutil      55. 47.100 / 55. 47.100
  libavcodec     57. 80.100 / 57. 80.100
  libavformat    57. 66.102 / 57. 66.102
  libavdevice    57.  2.100 / 57.  2.100
  libavfilter     6. 73.100 /  6. 73.100
  libswscale      4.  3.101 /  4.  3.101
  libswresample   2.  4.100 /  2.  4.100
  libpostproc    54.  2.100 / 54.  2.100
Input #0, mpeg, from 'E:\Track 02.mpg':
  Duration: 00:00:44.00, start: 0.373378, bitrate: 1402 kb/s
    Stream #0:0[0x1e0]: Video: mpeg1video, yuv420p(tv), 352x288 [SAR 178:163 DAR 1958:1467], 1150 kb/s, 25 fps, 25 tbr, 90k tbn, 25 tbc
    Stream #0:1[0x1c0]: Audio: mp2, 44100 Hz, stereo, s16p, 224 kb/s
[libx264 @ 000000000269e480] using SAR=178/163
[libx264 @ 000000000269e480] using cpu capabilities: MMX2 SSE2Fast SSSE3 SSE4.2 AVX FMA3 AVX2 LZCNT BMI2
[libx264 @ 000000000269e480] profile High, level 1.3
[libx264 @ 000000000269e480] 264 - core 148 r2762 90a61ec - H.264/MPEG-4 AVC codec - Copyleft 2003-2017 - http://www.videolan.org/x264.html - options: cabac=1 ref=3 deblock=1:0:0 analyse=0x3:0x113 me=hex subme=7 psy=1 psy_rd=1.00:0.00 mixed_ref=1 me_range=16 chroma_me=1 trellis=1 8x8dct=1 cqm=0 deadzone=21,11 fast_pskip=1 chroma_qp_offset=-2 threads=9 lookahead_threads=1 sliced_threads=0 nr=0 decimate=1 interlaced=0 bluray_compat=0 constrained_intra=0 bframes=3 b_pyramid=2 b_adapt=1 b_bias=0 direct=1 weightb=1 open_gop=0 weightp=2 keyint=250 keyint_min=25 scenecut=40 intra_refresh=0 rc_lookahead=40 rc=crf mbtree=1 crf=23.0 qcomp=0.60 qpmin=0 qpmax=69 qpstep=4 ip_ratio=1.40 aq=1:1.00
Output #0, mp4, to 'C:\Test.mp4':
";

    public const string FFmpegInfoFrameCount = @"ffmpeg version N-83507-g8fa18e0 Copyright (c) 2000-2017 the FFmpeg developers
  built with gcc 5.4.0 (GCC)
  configuration: --enable-gpl --enable-version3 --enable-cuda --enable-cuvid --enable-d3d11va --enable-dxva2 --enable-libmfx --enable-nvenc --enable-avisynth --enable-bzlib --enable-fontconfig --enable-frei0r --enable-gnutls --enable-iconv --enable-libass --enable-libbluray --enable-libbs2b --enable-libcaca --enable-libfreetype --enable-libgme --enable-libgsm --enable-libilbc --enable-libmodplug --enable-libmp3lame --enable-libopencore-amrnb --enable-libopencore-amrwb --enable-libopenh264 --enable-libopenjpeg --enable-libopus --enable-librtmp --enable-libsnappy --enable-libsoxr --enable-libspeex --enable-libtheora --enable-libtwolame --enable-libvidstab --enable-libvo-amrwbenc --enable-libvorbis --enable-libvpx --enable-libwavpack --enable-libwebp --enable-libx264 --enable-libx265 --enable-libxavs --enable-libxvid --enable-libzimg --enable-lzma --enable-zlib
  libavutil      55. 47.100 / 55. 47.100
  libavcodec     57. 80.100 / 57. 80.100
  libavformat    57. 66.102 / 57. 66.102
  libavdevice    57.  2.100 / 57.  2.100
  libavfilter     6. 73.100 /  6. 73.100
  libswscale      4.  3.101 /  4.  3.101
  libswresample   2.  4.100 /  2.  4.100
  libpostproc    54.  2.100 / 54.  2.100
Input #0, mov,mp4,m4a,3gp,3g2,mj2, from 'C:\GitHub\FFmpeg\FFmpeg.IntegrationTests\bin\Debug\SampleFiles\Feline Power.h264':
  Metadata:
    major_brand     : dash
    minor_version   : 0
    compatible_brands: iso6avc1mp41
    creation_time   : 2017-10-07T01:38:04.000000Z
  Duration: 00:00:14.60, start: 0.000000, bitrate: 311 kb/s
    Stream #0:0(und): Video: h264 (Main) (avc1 / 0x31637661), yuv420p(tv, bt709, progressive), 270x480 [SAR 1:1 DAR 9:16], 105 kb/s, 30 fps, 30 tbr, 90k tbn, 60 tbc (default)
    Metadata:
      creation_time   : 2017-10-07T01:38:04.000000Z
      handler_name    : VideoHandler
Output #0, null, to '/dev/null':
  Metadata:
    major_brand     : dash
    minor_version   : 0
    compatible_brands: iso6avc1mp41
    encoder         : Lavf57.66.102
    Stream #0:0(und): Video: wrapped_avframe, yuv420p, 270x480 [SAR 1:1 DAR 9:16], q=2-31, 200 kb/s, 30 fps, 30 tbn, 30 tbc (default)
    Metadata:
      creation_time   : 2017-10-07T01:38:04.000000Z
      handler_name    : VideoHandler
      encoder         : Lavc57.80.100 wrapped_avframe
Stream mapping:
  Stream #0:0 -> #0:0 (h264 (native) -> wrapped_avframe (native))
Press [q] to stop, [?] for help
frame=  438 fps=0.0 q=-0.0 Lsize=N/A time=00:00:14.60 bitrate=N/A speed= 222x    
video:212kB audio:0kB subtitle:0kB other streams:0kB global headers:0kB muxing overhead: unknown
";

    public const string FFmpegEncode1 = @"ffmpeg version N-83507-g8fa18e0 Copyright (c) 2000-2017 the FFmpeg developers
  built with gcc 5.4.0 (GCC)
  configuration: --enable-gpl --enable-version3 --enable-cuda --enable-cuvid --enable-d3d11va --enable-dxva2 --enable-libmfx --enable-nvenc --enable-avisynth --enable-bzlib --enable-fontconfig --enable-frei0r --enable-gnutls --enable-iconv --enable-libass --enable-libbluray --enable-libbs2b --enable-libcaca --enable-libfreetype --enable-libgme --enable-libgsm --enable-libilbc --enable-libmodplug --enable-libmp3lame --enable-libopencore-amrnb --enable-libopencore-amrwb --enable-libopenh264 --enable-libopenjpeg --enable-libopus --enable-librtmp --enable-libsnappy --enable-libsoxr --enable-libspeex --enable-libtheora --enable-libtwolame --enable-libvidstab --enable-libvo-amrwbenc --enable-libvorbis --enable-libvpx --enable-libwavpack --enable-libwebp --enable-libx264 --enable-libx265 --enable-libxavs --enable-libxvid --enable-libzimg --enable-lzma --enable-zlib
  libavutil      55. 47.100 / 55. 47.100
  libavcodec     57. 80.100 / 57. 80.100
  libavformat    57. 66.102 / 57. 66.102
  libavdevice    57.  2.100 / 57.  2.100
  libavfilter     6. 73.100 /  6. 73.100
  libswscale      4.  3.101 /  4.  3.101
  libswresample   2.  4.100 /  2.  4.100
  libpostproc    54.  2.100 / 54.  2.100
Input #0, mpeg, from 'E:\NaturalGrounding\Bie\.mpg\Track 03.mpg':
  Duration: 00:04:46.54, start: 0.373378, bitrate: 1395 kb/s
    Stream #0:0[0x1e0]: Video: mpeg1video, yuv420p(tv), 352x288 [SAR 178:163 DAR 1958:1467], 1150 kb/s, 25 fps, 25 tbr, 90k tbn, 25 tbc
    Stream #0:1[0x1c0]: Audio: mp2, 44100 Hz, stereo, s16p, 224 kb/s
[libx264 @ 00000000027d99c0] using SAR=178/163
[libx264 @ 00000000027d99c0] using cpu capabilities: MMX2 SSE2Fast SSSE3 SSE4.2 AVX FMA3 AVX2 LZCNT BMI2
[libx264 @ 00000000027d99c0] profile High, level 1.3
[libx264 @ 00000000027d99c0] 264 - core 148 r2762 90a61ec - H.264/MPEG-4 AVC codec - Copyleft 2003-2017 - http://www.videolan.org/x264.html - options: cabac=1 ref=3 deblock=1:0:0 analyse=0x3:0x113 me=hex subme=7 psy=1 psy_rd=1.00:0.00 mixed_ref=1 me_range=16 chroma_me=1 trellis=1 8x8dct=1 cqm=0 deadzone=21,11 fast_pskip=1 chroma_qp_offset=-2 threads=9 lookahead_threads=1 sliced_threads=0 nr=0 decimate=1 interlaced=0 bluray_compat=0 constrained_intra=0 bframes=3 b_pyramid=2 b_adapt=1 b_bias=0 direct=1 weightb=1 open_gop=0 weightp=2 keyint=250 keyint_min=25 scenecut=40 intra_refresh=0 rc_lookahead=40 rc=crf mbtree=1 crf=23.0 qcomp=0.60 qpmin=0 qpmax=69 qpstep=4 ip_ratio=1.40 aq=1:1.00
Output #0, mp4, to 'C:\Users\Etienne\Desktop\test.mp4':
  Metadata:
    encoder         : Lavf57.66.102
    Stream #0:0: Video: h264 (libx264) ([33][0][0][0] / 0x0021), yuv420p, 352x288 [SAR 178:163 DAR 1958:1467], q=-1--1, 25 fps, 12800 tbn, 25 tbc
    Metadata:
      encoder         : Lavc57.80.100 libx264
    Side data:
      cpb: bitrate max/min/avg: 0/0/0 buffer size: 0 vbv_delay: -1
    Stream #0:1: Audio: aac (LC) ([64][0][0][0] / 0x0040), 44100 Hz, stereo, fltp, 128 kb/s
    Metadata:
      encoder         : Lavc57.80.100 aac
Stream mapping:
  Stream #0:0 -> #0:0 (mpeg1video (native) -> h264 (libx264))
  Stream #0:1 -> #0:1 (mp2 (native) -> aac (native))
Press [q] to stop, [?] for help
frame=  170 fps=0.0 q=28.0 size=     139kB time=00:00:06.59 bitrate= 172.1kbits/s speed=13.2x    
frame=  432 fps=430 q=28.0 size=     580kB time=00:00:17.11 bitrate= 277.7kbits/s speed=  17x    
frame=  690 fps=458 q=28.0 size=    1050kB time=00:00:27.37 bitrate= 314.3kbits/s speed=18.2x    
frame=  901 fps=448 q=28.0 size=    1425kB time=00:00:35.73 bitrate= 326.7kbits/s speed=17.8x    
frame= 1123 fps=447 q=28.0 size=    1770kB time=00:00:44.67 bitrate= 324.6kbits/s speed=17.8x    
frame= 1361 fps=451 q=28.0 size=    2279kB time=00:00:54.31 bitrate= 343.8kbits/s speed=  18x    
frame= 1612 fps=458 q=28.0 size=    2815kB time=00:01:04.38 bitrate= 358.1kbits/s speed=18.3x    
frame= 1870 fps=466 q=28.0 size=    3391kB time=00:01:14.48 bitrate= 372.9kbits/s speed=18.5x    
frame= 2119 fps=469 q=28.0 size=    3825kB time=00:01:24.61 bitrate= 370.3kbits/s speed=18.7x    
frame= 2388 fps=476 q=28.0 size=    4253kB time=00:01:35.31 bitrate= 365.5kbits/s speed=  19x    
frame= 2677 fps=485 q=28.0 size=    4700kB time=00:01:47.04 bitrate= 359.7kbits/s speed=19.4x    
frame= 2945 fps=489 q=28.0 size=    5302kB time=00:01:57.56 bitrate= 369.4kbits/s speed=19.5x    
frame= 3207 fps=492 q=28.0 size=    5690kB time=00:02:08.15 bitrate= 363.8kbits/s speed=19.7x    
frame= 3465 fps=494 q=28.0 size=    6132kB time=00:02:18.43 bitrate= 362.8kbits/s speed=19.7x    
frame= 3715 fps=494 q=28.0 size=    6596kB time=00:02:28.32 bitrate= 364.3kbits/s speed=19.7x    
frame= 3974 fps=496 q=28.0 size=    7002kB time=00:02:38.77 bitrate= 361.3kbits/s speed=19.8x    
frame= 4221 fps=495 q=28.0 size=    7455kB time=00:02:48.73 bitrate= 361.9kbits/s speed=19.8x    
frame= 4459 fps=494 q=28.0 size=    7899kB time=00:02:58.16 bitrate= 363.2kbits/s speed=19.8x    
frame= 4721 fps=496 q=28.0 size=    8328kB time=00:03:08.66 bitrate= 361.6kbits/s speed=19.8x    
frame= 4986 fps=498 q=28.0 size=    8900kB time=00:03:19.25 bitrate= 365.9kbits/s speed=19.9x    
frame= 5271 fps=501 q=28.0 size=    9363kB time=00:03:30.55 bitrate= 364.3kbits/s speed=  20x    
frame= 5559 fps=504 q=28.0 size=    9797kB time=00:03:42.14 bitrate= 361.3kbits/s speed=20.2x    
frame= 5821 fps=505 q=28.0 size=   10319kB time=00:03:52.68 bitrate= 363.3kbits/s speed=20.2x    
frame= 6091 fps=507 q=28.0 size=   10724kB time=00:04:03.41 bitrate= 360.9kbits/s speed=20.2x    
frame= 6340 fps=506 q=28.0 size=   11234kB time=00:04:13.30 bitrate= 363.3kbits/s speed=20.2x    
frame= 6618 fps=508 q=28.0 size=   11628kB time=00:04:24.56 bitrate= 360.0kbits/s speed=20.3x    
frame= 6878 fps=509 q=28.0 size=   12117kB time=00:04:35.06 bitrate= 360.9kbits/s speed=20.3x    
frame= 7151 fps=510 q=28.0 size=   12561kB time=00:04:45.76 bitrate= 360.1kbits/s speed=20.4x    
frame= 7163 fps=508 q=-1.0 Lsize=   12860kB time=00:04:46.55 bitrate= 367.6kbits/s speed=20.3x    
video:8005kB audio:4627kB subtitle:0kB other streams:0kB global headers:0kB muxing overhead: 1.803926%
[libx264 @ 00000000027d99c0] frame I:72    Avg QP:18.88  size:  8670
[libx264 @ 00000000027d99c0] frame P:2352  Avg QP:22.53  size:  2416
[libx264 @ 00000000027d99c0] frame B:4739  Avg QP:25.60  size:   399
[libx264 @ 00000000027d99c0] consecutive B-frames:  3.6% 10.4% 42.5% 43.5%
[libx264 @ 00000000027d99c0] mb I  I16..4: 30.3% 52.7% 17.0%
[libx264 @ 00000000027d99c0] mb P  I16..4:  3.8%  7.4%  1.5%  P16..4: 34.4% 11.5%  7.1%  0.0%  0.0%    skip:34.3%
[libx264 @ 00000000027d99c0] mb B  I16..4:  0.3%  0.5%  0.0%  B16..8: 24.0%  2.9%  0.6%  direct: 2.0%  skip:69.7%  L0:37.2% L1:53.3% BI: 9.5%
[libx264 @ 00000000027d99c0] 8x8 transform intra:57.9% inter:76.2%
[libx264 @ 00000000027d99c0] coded y,uvDC,uvAC intra: 46.9% 63.2% 25.5% inter: 9.0% 13.3% 1.3%
[libx264 @ 00000000027d99c0] i16 v,h,dc,p: 39% 47%  3% 11%
[libx264 @ 00000000027d99c0] i8 v,h,dc,ddl,ddr,vr,hd,vl,hu: 28% 18% 30%  3%  3%  5%  3%  5%  4%
[libx264 @ 00000000027d99c0] i4 v,h,dc,ddl,ddr,vr,hd,vl,hu: 31% 25% 12%  4%  6%  7%  5%  5%  5%
[libx264 @ 00000000027d99c0] i8c dc,h,v,p: 44% 25% 25%  6%
[libx264 @ 00000000027d99c0] Weighted P-Frames: Y:3.2% UV:1.8%
[libx264 @ 00000000027d99c0] ref P L0: 65.5% 13.5% 15.6%  5.3%  0.1%
[libx264 @ 00000000027d99c0] ref B L0: 89.8%  8.3%  1.9%
[libx264 @ 00000000027d99c0] ref B L1: 98.3%  1.7%
[libx264 @ 00000000027d99c0] kb/s:228.85
[aac @ 0000000002635480] Qavg: 451.083
";

    public const string FFmpegEncode2 = @"ffmpeg version N-83507-g8fa18e0 Copyright (c) 2000-2017 the FFmpeg developers
  built with gcc 5.4.0 (GCC)
  configuration: --enable-gpl --enable-version3 --enable-cuda --enable-cuvid --enable-d3d11va --enable-dxva2 --enable-libmfx --enable-nvenc --enable-avisynth --enable-bzlib --enable-fontconfig --enable-frei0r --enable-gnutls --enable-iconv --enable-libass --enable-libbluray --enable-libbs2b --enable-libcaca --enable-libfreetype --enable-libgme --enable-libgsm --enable-libilbc --enable-libmodplug --enable-libmp3lame --enable-libopencore-amrnb --enable-libopencore-amrwb --enable-libopenh264 --enable-libopenjpeg --enable-libopus --enable-librtmp --enable-libsnappy --enable-libsoxr --enable-libspeex --enable-libtheora --enable-libtwolame --enable-libvidstab --enable-libvo-amrwbenc --enable-libvorbis --enable-libvpx --enable-libwavpack --enable-libwebp --enable-libx264 --enable-libx265 --enable-libxavs --enable-libxvid --enable-libzimg --enable-lzma --enable-zlib
  libavutil      55. 47.100 / 55. 47.100
  libavcodec     57. 80.100 / 57. 80.100
  libavformat    57. 66.102 / 57. 66.102
  libavdevice    57.  2.100 / 57.  2.100
  libavfilter     6. 73.100 /  6. 73.100
  libswscale      4.  3.101 /  4.  3.101
  libswresample   2.  4.100 /  2.  4.100
  libpostproc    54.  2.100 / 54.  2.100
Input #0, mov,mp4,m4a,3gp,3g2,mj2, from 'C:\Users\Etienne\Desktop\test.mp4':
  Metadata:
    major_brand     : isom
    minor_version   : 512
    compatible_brands: isomiso2avc1mp41
    encoder         : Lavf57.66.102
  Duration: 00:04:46.56, start: 0.000000, bitrate: 367 kb/s
    Stream #0:0(und): Video: h264 (High) (avc1 / 0x31637661), yuv420p, 352x288 [SAR 178:163 DAR 1958:1467], 228 kb/s, 25 fps, 25 tbr, 12800 tbn, 50 tbc (default)
    Metadata:
      handler_name    : VideoHandler
    Stream #0:1(und): Audio: aac (LC) (mp4a / 0x6134706D), 44100 Hz, stereo, fltp, 132 kb/s (default)
    Metadata:
      handler_name    : SoundHandler
[libx264 @ 0000000002698f80] using SAR=178/163
[libx264 @ 0000000002698f80] using cpu capabilities: MMX2 SSE2Fast SSSE3 SSE4.2 AVX FMA3 AVX2 LZCNT BMI2
[libx264 @ 0000000002698f80] profile High, level 1.3
[libx264 @ 0000000002698f80] 264 - core 148 r2762 90a61ec - H.264/MPEG-4 AVC codec - Copyleft 2003-2017 - http://www.videolan.org/x264.html - options: cabac=1 ref=3 deblock=1:0:0 analyse=0x3:0x113 me=hex subme=7 psy=1 psy_rd=1.00:0.00 mixed_ref=1 me_range=16 chroma_me=1 trellis=1 8x8dct=1 cqm=0 deadzone=21,11 fast_pskip=1 chroma_qp_offset=-2 threads=9 lookahead_threads=1 sliced_threads=0 nr=0 decimate=1 interlaced=0 bluray_compat=0 constrained_intra=0 bframes=3 b_pyramid=2 b_adapt=1 b_bias=0 direct=1 weightb=1 open_gop=0 weightp=2 keyint=250 keyint_min=25 scenecut=40 intra_refresh=0 rc_lookahead=40 rc=crf mbtree=1 crf=23.0 qcomp=0.60 qpmin=0 qpmax=69 qpstep=4 ip_ratio=1.40 aq=1:1.00
Output #0, mp4, to 'C:\Users\Etienne\Desktop\test2.mp4':
  Metadata:
    major_brand     : isom
    minor_version   : 512
    compatible_brands: isomiso2avc1mp41
    encoder         : Lavf57.66.102
    Stream #0:0(und): Video: h264 (libx264) ([33][0][0][0] / 0x0021), yuv420p, 352x288 [SAR 178:163 DAR 1958:1467], q=-1--1, 25 fps, 12800 tbn, 25 tbc (default)
    Metadata:
      handler_name    : VideoHandler
      encoder         : Lavc57.80.100 libx264
    Side data:
      cpb: bitrate max/min/avg: 0/0/0 buffer size: 0 vbv_delay: -1
    Stream #0:1(und): Audio: aac (LC) ([64][0][0][0] / 0x0040), 44100 Hz, stereo, fltp, 128 kb/s (default)
    Metadata:
      handler_name    : SoundHandler
      encoder         : Lavc57.80.100 aac
Stream mapping:
  Stream #0:0 -> #0:0 (h264 (native) -> h264 (libx264))
  Stream #0:1 -> #0:1 (aac (native) -> aac (native))
Press [q] to stop, [?] for help
frame=  195 fps=0.0 q=28.0 size=     163kB time=00:00:08.08 bitrate= 165.3kbits/s speed=16.1x    
frame=  439 fps=439 q=28.0 size=     564kB time=00:00:17.85 bitrate= 258.8kbits/s speed=17.8x    
frame=  697 fps=464 q=28.0 size=    1021kB time=00:00:28.16 bitrate= 296.8kbits/s speed=18.8x    
frame=  961 fps=480 q=28.0 size=    1457kB time=00:00:38.68 bitrate= 308.6kbits/s speed=19.3x    
frame= 1214 fps=485 q=28.0 size=    1876kB time=00:00:48.85 bitrate= 314.6kbits/s speed=19.5x    
frame= 1468 fps=489 q=28.0 size=    2382kB time=00:00:59.00 bitrate= 330.7kbits/s speed=19.6x    
frame= 1730 fps=494 q=28.0 size=    2919kB time=00:01:09.45 bitrate= 344.3kbits/s speed=19.8x    
frame= 2024 fps=505 q=28.0 size=    3458kB time=00:01:21.24 bitrate= 348.7kbits/s speed=20.3x    
frame= 2319 fps=515 q=28.0 size=    3918kB time=00:01:33.04 bitrate= 345.0kbits/s speed=20.6x    
frame= 2615 fps=522 q=28.0 size=    4360kB time=00:01:44.83 bitrate= 340.7kbits/s speed=20.9x    
frame= 2899 fps=526 q=28.0 size=    4925kB time=00:01:56.21 bitrate= 347.1kbits/s speed=21.1x    
frame= 3199 fps=532 q=28.0 size=    5382kB time=00:02:08.24 bitrate= 343.8kbits/s speed=21.3x    
frame= 3469 fps=533 q=28.0 size=    5813kB time=00:02:19.04 bitrate= 342.5kbits/s speed=21.4x    
frame= 3727 fps=532 q=28.0 size=    6259kB time=00:02:29.37 bitrate= 343.2kbits/s speed=21.3x    
frame= 3993 fps=532 q=28.0 size=    6654kB time=00:02:40.00 bitrate= 340.6kbits/s speed=21.3x    
frame= 4261 fps=532 q=28.0 size=    7118kB time=00:02:50.68 bitrate= 341.6kbits/s speed=21.3x    
frame= 4533 fps=533 q=28.0 size=    7576kB time=00:03:01.55 bitrate= 341.8kbits/s speed=21.3x    
frame= 4819 fps=535 q=28.0 size=    8049kB time=00:03:13.00 bitrate= 341.6kbits/s speed=21.4x    
frame= 5109 fps=537 q=28.0 size=    8611kB time=00:03:24.61 bitrate= 344.8kbits/s speed=21.5x    
frame= 5406 fps=540 q=28.0 size=    9056kB time=00:03:36.52 bitrate= 342.6kbits/s speed=21.6x    
frame= 5695 fps=542 q=28.0 size=    9555kB time=00:03:48.04 bitrate= 343.3kbits/s speed=21.7x    
frame= 6002 fps=545 q=28.0 size=   10002kB time=00:04:00.34 bitrate= 340.9kbits/s speed=21.8x    
frame= 6283 fps=546 q=28.0 size=   10540kB time=00:04:11.56 bitrate= 343.2kbits/s speed=21.8x    
frame= 6545 fps=545 q=28.0 size=   10927kB time=00:04:22.03 bitrate= 341.6kbits/s speed=21.8x    
frame= 6830 fps=546 q=28.0 size=   11394kB time=00:04:33.46 bitrate= 341.3kbits/s speed=21.8x    
frame= 7118 fps=547 q=28.0 size=   11868kB time=00:04:45.00 bitrate= 341.1kbits/s speed=21.9x    
frame= 7163 fps=546 q=-1.0 Lsize=   12215kB time=00:04:46.55 bitrate= 349.2kbits/s speed=21.8x    
video:7403kB audio:4582kB subtitle:0kB other streams:0kB global headers:0kB muxing overhead: 1.915474%
[libx264 @ 0000000002698f80] frame I:70    Avg QP:18.37  size:  8747
[libx264 @ 0000000002698f80] frame P:2867  Avg QP:21.81  size:  1908
[libx264 @ 0000000002698f80] frame B:4226  Avg QP:24.91  size:   355
[libx264 @ 0000000002698f80] consecutive B-frames:  8.6% 28.8% 28.0% 34.6%
[libx264 @ 0000000002698f80] mb I  I16..4: 31.0% 50.8% 18.2%
[libx264 @ 0000000002698f80] mb P  I16..4:  2.9%  6.4%  1.2%  P16..4: 31.0% 10.2%  5.5%  0.0%  0.0%    skip:42.9%
[libx264 @ 0000000002698f80] mb B  I16..4:  0.2%  0.4%  0.0%  B16..8: 22.1%  2.8%  0.6%  direct: 2.1%  skip:71.7%  L0:37.5% L1:54.1% BI: 8.4%
[libx264 @ 0000000002698f80] 8x8 transform intra:59.2% inter:75.9%
[libx264 @ 0000000002698f80] coded y,uvDC,uvAC intra: 43.7% 61.1% 24.0% inter: 7.8% 12.8% 1.1%
[libx264 @ 0000000002698f80] i16 v,h,dc,p: 39% 46%  3% 11%
[libx264 @ 0000000002698f80] i8 v,h,dc,ddl,ddr,vr,hd,vl,hu: 27% 17% 30%  3%  4%  5%  4%  5%  4%
[libx264 @ 0000000002698f80] i4 v,h,dc,ddl,ddr,vr,hd,vl,hu: 32% 25% 12%  4%  6%  7%  5%  5%  4%
[libx264 @ 0000000002698f80] i8c dc,h,v,p: 44% 25% 25%  6%
[libx264 @ 0000000002698f80] Weighted P-Frames: Y:2.2% UV:1.6%
[libx264 @ 0000000002698f80] ref P L0: 68.2% 13.8% 12.9%  5.0%  0.1%
[libx264 @ 0000000002698f80] ref B L0: 90.1%  8.2%  1.7%
[libx264 @ 0000000002698f80] ref B L1: 98.9%  1.1%
[libx264 @ 0000000002698f80] kb/s:211.65
[aac @ 0000000002693bc0] Qavg: 398.248
";

    public const string FFmpegEncode3 = @"ffmpeg version N-93217-ga899b3b3c5 Copyright (c) 2000-2019 the FFmpeg developers
  built with gcc 8.2.1 (GCC) 20190212
  configuration: --enable-gpl --enable-version3 --enable-sdl2 --enable-fontconfig --enable-gnutls --enable-iconv --enable-libass --enable-libdav1d --enable-libbluray --enable-libfreetype --enable-libmp3lame --enable-libopencore-amrnb --enable-libopencore-amrwb --enable-libopenjpeg --enable-libopus --enable-libshine --enable-libsnappy --enable-libsoxr --enable-libtheora --enable-libtwolame --enable-libvpx --enable-libwavpack --enable-libwebp --enable-libx264 --enable-libx265 --enable-libxml2 --enable-libzimg --enable-lzma --enable-zlib --enable-gmp --enable-libvidstab --enable-libvorbis --enable-libvo-amrwbenc --enable-libmysofa --enable-libspeex --enable-libxvid --enable-libaom --enable-libmfx --enable-amf --enable-ffnvcodec --enable-cuvid --enable-d3d11va --enable-nvenc --enable-nvdec --enable-dxva2 --enable-avisynth --enable-libopenmpt
  libavutil      56. 26.100 / 56. 26.100
  libavcodec     58. 47.102 / 58. 47.102
  libavformat    58. 26.101 / 58. 26.101
  libavdevice    58.  6.101 / 58.  6.101
  libavfilter     7. 48.100 /  7. 48.100
  libswscale      5.  4.100 /  5.  4.100
  libswresample   3.  4.100 /  3.  4.100
  libpostproc    55.  4.100 / 55.  4.100
Input #0, yuv4mpegpipe, from 'pipe:':
  Duration: N/A, start: 0.000000, bitrate: N/A
    Stream #0:0: Video: rawvideo (I420 / 0x30323449), yuv420p(progressive), 640x480, 24 fps, 24 tbr, 24 tbn, 24 tbc
Stream mapping:
  Stream #0:0 -> #0:0 (rawvideo (native) -> h264 (libx264))
[libx264 @ 0000023c6f2367c0] using cpu capabilities: MMX2 SSE2Fast SSSE3 SSE4.2 AVX FMA3 BMI2 AVX2
[libx264 @ 0000023c6f2367c0] profile Constrained Baseline, level 3.0, 4:2:0, 8-bit
[libx264 @ 0000023c6f2367c0] 264 - core 157 r2935 545de2f - H.264/MPEG-4 AVC codec - Copyleft 2003-2018 - http://www.videolan.org/x264.html - options: cabac=0 ref=1 deblock=0:0:0 analyse=0:0 me=dia subme=0 psy=1 psy_rd=1.00:0.00 mixed_ref=0 me_range=16 chroma_me=1 trellis=1 8x8dct=0 cqm=0 deadzone=21,11 fast_pskip=1 chroma_qp_offset=0 threads=12 lookahead_threads=2 sliced_threads=0 nr=0 decimate=1 interlaced=0 bluray_compat=0 constrained_intra=0 bframes=0 weightp=0 keyint=250 keyint_min=24 scenecut=0 intra_refresh=0 rc=crf mbtree=0 crf=23.0 qcomp=0.60 qpmin=0 qpmax=69 qpstep=4 ip_ratio=1.40 aq=0
Output #0, mp4, to 'C:\GitHub\FFmpeg\FFmpeg.IntegrationTests\bin\Debug\Output\EncodeAvisynthToFFmpeg AvisynthLong.mp4':
  Metadata:
    encoder         : Lavf58.26.101
    Stream #0:0: Video: h264 (libx264) (avc1 / 0x31637661), yuv420p, 640x480, q=-1--1, 24 fps, 12288 tbn, 24 tbc
    Metadata:
      encoder         : Lavc58.47.102 libx264
    Side data:
      cpb: bitrate max/min/avg: 0/0/0 buffer size: 0 vbv_delay: -1
frame=  725 fps=0.0 q=12.0 size=       0kB time=00:00:29.62 bitrate=   0.0kbits/s speed=59.2x    
frame= 1452 fps=1451 q=12.0 size=       0kB time=00:00:59.91 bitrate=   0.0kbits/s speed=59.9x    
frame= 2141 fps=1426 q=12.0 size=       0kB time=00:01:28.62 bitrate=   0.0kbits/s speed=  59x    
frame= 2859 fps=1429 q=12.0 size=       0kB time=00:01:58.54 bitrate=   0.0kbits/s speed=59.2x    
frame= 3664 fps=1465 q=12.0 size=       0kB time=00:02:32.08 bitrate=   0.0kbits/s speed=60.8x    
frame= 4452 fps=1483 q=12.0 size=       0kB time=00:03:04.91 bitrate=   0.0kbits/s speed=61.6x    
frame= 5218 fps=1490 q=12.0 size=       0kB time=00:03:36.83 bitrate=   0.0kbits/s speed=61.9x    
frame= 5993 fps=1497 q=12.0 size=       0kB time=00:04:09.12 bitrate=   0.0kbits/s speed=62.2x    
frame= 6756 fps=1500 q=12.0 size=       0kB time=00:04:40.91 bitrate=   0.0kbits/s speed=62.4x    
avs2pipemod[info]: finished, wrote 100000 frames [100%].
avs2pipemod[info]: total elapsed time is 65.339 sec.
frame=100000 fps=1531 q=-1.0 Lsize=    1828kB time=01:09:26.62 bitrate=   3.6kbits/s speed=63.8x    
video:1435kB audio:0kB subtitle:0kB other streams:0kB global headers:0kB muxing overhead: 27.390961%
[libx264 @ 0000023c6f2367c0] frame I:400   Avg QP: 9.03  size:   932
[libx264 @ 0000023c6f2367c0] frame P:99600 Avg QP:12.01  size:    11
[libx264 @ 0000023c6f2367c0] mb I  I16..4: 100.0%  0.0%  0.0%
[libx264 @ 0000023c6f2367c0] mb P  I16..4:  0.0%  0.0%  0.0%  P16..4:  0.0%  0.0%  0.0%  0.0%  0.0%    skip:100.0%
[libx264 @ 0000023c6f2367c0] coded y,uvDC,uvAC intra: 0.0% 0.1% 0.0% inter: 0.0% 0.0% 0.0%
[libx264 @ 0000023c6f2367c0] i16 v,h,dc,p: 97%  0%  3%  0%
[libx264 @ 0000023c6f2367c0] i8c dc,h,v,p: 100%  0%  0%  0%
[libx264 @ 0000023c6f2367c0] kb/s:2.82";

    public const string X264Encode1 = @"[info]: indexing input file [11.5%] 
                                    
ffms [info]: 270x480p 1:1 @ 30/1 fps (vfr)
x264 [info]: using SAR=1/1
x264 [info]: using cpu capabilities: MMX2 SSE2Fast SSSE3 SSE4.2 AVX FMA3 BMI2 AVX2
x264 [info]: profile High 10, level 2.1, 4:2:0 10-bit
x264 [info]: cabac=1 ref=3 deblock=1:0:0 analyse=0x3:0x113 me=hex subme=7 psy=1 fade_compensate=0.00 psy_rd=1.00:0.00 mixed_ref=1 me_range=16 chroma_me=1 trellis=1 8x8dct=1 cqm=0 deadzone=21,11 fast_pskip=1 chroma_qp_offset=-2 threads=12 lookahead_threads=2 sliced_threads=0 nr=0 decimate=1 interlaced=0 bluray_compat=0 constrained_intra=0 bframes=3 b_pyramid=2 b_adapt=1 b_bias=0 direct=1 weightb=1 open_gop=0 weightp=2 keyint=250 keyint_min=25 scenecut=40 intra_refresh=0 rc_lookahead=40 rc=crf mbtree=1 crf=23.0000 qcomp=0.60 qpmin=0 qpmax=81 qpstep=4 ip_ratio=1.40 aq=1:1.00
             frames     fps     kb/s    elapsed   remain     size    est.size
[  0.2%]      1/438     9.71  4423.20   0:00:00   0:00:45   18.00 KB    7.70 MB  
[ 18.7%]     82/438    232.29   473.06   0:00:00   0:00:01  157.84 KB  843.10 KB  
[ 46.1%]    202/438    334.44   325.56   0:00:00   0:00:00  267.59 KB  580.22 KB  
[ 70.1%]    307/438    359.48   335.21   0:00:00   0:00:00  418.73 KB  597.41 KB  
[ 90.0%]    394/438    354.95   354.12   0:00:01   0:00:00  567.71 KB  631.11 KB  
[100.0%]    438/438    364.70   360.78   0:00:01   0:00:00  643.00 KB  643.00 KB  
x264 [info]: frame I:2     Avg QP:33.80  size: 18946
x264 [info]: frame P:134   Avg QP:35.59  size:  3405
x264 [info]: frame B:302   Avg QP:40.84  size:   541
x264 [info]: consecutive B-frames:  5.7%  4.6%  7.5% 82.2%
x264 [info]: mb I  I16..4: 13.0% 34.5% 52.5%
x264 [info]: mb P  I16..4:  7.1%  6.8%  1.5%  P16..4: 39.3% 17.2% 10.9%  0.0%  0.0%    skip:17.2%
x264 [info]: mb B  I16..4:  0.1%  0.2%  0.1%  B16..8: 44.4%  4.2%  0.6%  direct: 0.8%  skip:49.6%  L0:44.6% L1:46.6% BI: 8.8%
x264 [info]: 8x8 transform intra:43.6% inter:52.7%
x264 [info]: coded y,uvDC,uvAC intra: 32.1% 48.3% 6.9% inter: 12.8% 5.5% 0.1%
x264 [info]: i16 v,h,dc,p: 42% 19% 12% 27%
x264 [info]: i8 v,h,dc,ddl,ddr,vr,hd,vl,hu: 28% 18% 28%  3%  4%  4%  6%  5%  4%
x264 [info]: i4 v,h,dc,ddl,ddr,vr,hd,vl,hu: 24% 13% 18%  4% 10%  8% 11%  6%  6%
x264 [info]: i8c dc,h,v,p: 58% 18% 16%  7%
x264 [info]: Weighted P-Frames: Y:16.4% UV:3.0%
x264 [info]: ref P L0: 81.5% 13.4%  4.9%  0.1%
x264 [info]: ref B L0: 91.2%  7.5%  1.3%
x264 [info]: ref B L1: 97.3%  2.7%
x264 [info]: kb/s:360.37

encoded 438 frames, 364.70 fps, 360.78 kb/s, duration 0:00:01.20";

    public const string X264Encode2 = @"y4m [info]: 640x480p 0:0 @ 24/1 fps (cfr)
x264 [info]: using cpu capabilities: MMX2 SSE2Fast SSSE3 SSE4.2 AVX FMA3 BMI2 AVX2
x264 [info]: profile High 10, level 3.0, 4:2:0 10-bit
x264 [info]: cabac=0 ref=1 deblock=0:0:0 analyse=0:0 me=dia subme=0 psy=1 fade_compensate=0.00 psy_rd=1.00:0.00 mixed_ref=0 me_range=16 chroma_me=1 trellis=0 8x8dct=0 cqm=0 deadzone=21,11 fast_pskip=1 chroma_qp_offset=0 threads=12 lookahead_threads=2 sliced_threads=0 nr=0 decimate=1 interlaced=0 bluray_compat=0 constrained_intra=0 bframes=0 weightp=0 keyint=250 keyint_min=24 scenecut=0 intra_refresh=0 rc=crf mbtree=0 crf=23.0000 qcomp=0.60 qpmin=0 qpmax=81 qpstep=4 ip_ratio=1.40 aq=0
frames   fps      kb/s     elapsed     size
     1  43.48    304.90    0:00:00     1.55 KB  
   155  567.77      4.07    0:00:00     3.21 KB  
   333  636.71      3.55    0:00:00     6.02 KB  
   561  725.74      3.28    0:00:00     9.37 KB  
   796  778.10      3.16    0:00:01    12.79 KB  
  1020  801.26      3.10    0:00:01    16.10 KB  
  1208  793.17      2.95    0:00:01    18.12 KB  
  1418  799.77      2.95    0:00:01    21.27 KB  
  1632  806.72      2.95    0:00:02    24.47 KB  
avs2pipemod[info]: finished, wrote 10000 frames [100%].
avs2pipemod[info]: total elapsed time is 11.851 sec.
 10000  843.74      2.83    0:00:11   144.06 KB  
x264 [info]: frame I:40    Avg QP:21.27  size:   932
x264 [info]: frame P:9960  Avg QP:24.01  size:    11
x264 [info]: mb I  I16..4: 100.0%  0.0%  0.0%
x264 [info]: mb P  I16..4:  0.0%  0.0%  0.0%  P16..4:  0.0%  0.0%  0.0%  0.0%  0.0%    skip:100.0%
x264 [info]: coded y,uvDC,uvAC intra: 0.0% 0.1% 0.0% inter: 0.0% 0.0% 0.0%
x264 [info]: i16 v,h,dc,p: 97%  0%  3%  0%
x264 [info]: i8c dc,h,v,p: 100%  0%  0%  0%
x264 [info]: kb/s:2.82

encoded 10000 frames, 843.74 fps, 2.83 kb/s, duration 0:00:11.85";

    public const string X264Encode3 = @"y4m [info]: 640x480p 0:0 @ 24/1 fps (cfr)
x264 [info]: using cpu capabilities: MMX2 SSE2Fast SSSE3 SSE4.2 AVX FMA3 BMI2 AVX2
x264 [info]: profile High 10, level 3.0, 4:2:0 10-bit
x264 [info]: cabac=0 ref=1 deblock=0:0:0 analyse=0:0 me=dia subme=0 psy=1 fade_compensate=0.00 psy_rd=1.00:0.00 mixed_ref=0 me_range=16 chroma_me=1 trellis=0 8x8dct=0 cqm=0 deadzone=21,11 fast_pskip=1 chroma_qp_offset=0 threads=12 lookahead_threads=2 sliced_threads=0 nr=0 decimate=1 interlaced=0 bluray_compat=0 constrained_intra=0 bframes=0 weightp=0 keyint=250 keyint_min=24 scenecut=0 intra_refresh=0 rc=crf mbtree=0 crf=23.0000 qcomp=0.60 qpmin=0 qpmax=81 qpstep=4 ip_ratio=1.40 aq=0
             frames     fps     kb/s    elapsed   remain     size    est.size
[  0.0%]      1/1000000 41.67   304.90   0:00:00   6:39:59    1.55 KB 1514.43 MB  
[  0.0%]    195/1000000 711.68     3.66   0:00:00   0:23:24    3.63 KB   18.20 MB  
[  0.0%]    396/1000000 754.29     3.32   0:00:00   0:22:05    6.69 KB   16.51 MB  
[  0.1%]    615/1000000 793.55     3.18   0:00:00   0:20:59    9.95 KB   15.79 MB  
[  0.1%]    808/1000000 788.29     3.14   0:00:01   0:21:07   12.92 KB   15.62 MB  
[  0.1%]   1005/1000000 788.24     3.12   0:00:01   0:21:07   15.94 KB   15.48 MB  
[  0.1%]   1214/1000000 795.54     2.94   0:00:01   0:20:55   18.18 KB   14.62 MB  
[  0.1%]   1430/1000000 804.73     2.94   0:00:01   0:20:40   21.40 KB   14.62 MB  
[  0.2%]   1647/1000000 812.53     2.94   0:00:02   0:20:28   24.63 KB   14.61 MB  
avs2pipemod[info]: finished, wrote 10000 frames [100%].
avs2pipemod[info]: total elapsed time is 11.753 sec.
[  1.0%]  10000/1000000 850.99     2.83   0:00:11   0:19:23  144.06 KB   14.07 MB  
x264 [info]: frame I:40    Avg QP:21.27  size:   932
x264 [info]: frame P:9960  Avg QP:24.01  size:    11
x264 [info]: mb I  I16..4: 100.0%  0.0%  0.0%
x264 [info]: mb P  I16..4:  0.0%  0.0%  0.0%  P16..4:  0.0%  0.0%  0.0%  0.0%  0.0%    skip:100.0%
x264 [info]: coded y,uvDC,uvAC intra: 0.0% 0.1% 0.0% inter: 0.0% 0.0% 0.0%
x264 [info]: i16 v,h,dc,p: 97%  0%  3%  0%
x264 [info]: i8c dc,h,v,p: 100%  0%  0%  0%
x264 [info]: kb/s:2.82

encoded 10000 frames, 850.99 fps, 2.83 kb/s, duration 0:00:11.75";
}