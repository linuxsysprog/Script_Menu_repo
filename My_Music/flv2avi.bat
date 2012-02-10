ffmpeg -i "%~d1%~p1%~n1.flv" -vcodec rawvideo -acodec pcm_s16le -ar 44100 -ac 2 "%~d1%~p1%~n1.avi"
pause
