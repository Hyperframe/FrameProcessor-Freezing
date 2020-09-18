# FrameProcessor-Freezing

This repository is used to reproduce a fatal issue I am encountering while processing camera frames on the HL2 using MediaCapture. Intermittently, while frames are being processed, the device will freeze (all UI gone and app unresponsive). This error is unrecoverable and requires the application to be killed and restarted. This can happen anywhere from 250 frames processed to 3k, to over 10k. Frustratingly, sometimes it seems to never freeze. But the majority of the time I am able to get it to reproduce in under 10k frames.

This is a minimal frame processing application that often will reproduce the problem on my device. This application uses the latest Unity 2019.4.10f1 and MRTK 2.4.0.

To reproduce:
* Build and deploy the app to the HoloLens 2.
* Allow permissions and press the Start Capture button to begin processing frames.
* Wait for the application to freeze. You can see how many frames have been processed by attaching a debugger and viewing the logs.
