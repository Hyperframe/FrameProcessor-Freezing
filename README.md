# FrameProcessor-Freezing

On the HoloLens 2, when `Script Debugging` is enabled and you are processing frames with `MediaFrameReader`, intermittently the app will freeze (all UI gone and app is unresponsive). This error is unrecoverable and requires the application to be killed and restarted. This can happen anywhere from 250 frames processed to 3k, to over 10k. It seems that the amount of "code running" affects how quickly this problem reproduces. In my actual application with much more code, this problem reproduces 100% of the time within 10 seconds. In this minimal sample, it can take longer, but frequently reproduces. When `Script Debugging` is disabled, the problem never reproduces (even if Development mode is still enabled). This has been reproduced on multiple HoloLens 2 devices.

This minimal application uses the latest Unity 2019.4.11f1 and MRTK 2.4.0 (at time of writing).

To reproduce:
1. Build the app in `Development` with `Script Debugging` enabled.
2. Deploy the app to the HoloLens 2 (I deployed using the `Release` configuration).
3. Allow the microphone permission, press the Start Capture UI button, and allow the camera permission to begin processing frames.
4. Wait for the application to freeze. You can see how many frames have been processed by attaching a debugger and viewing the logs.
