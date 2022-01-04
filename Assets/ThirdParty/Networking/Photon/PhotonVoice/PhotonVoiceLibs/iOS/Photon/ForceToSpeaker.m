// force to speaker based on https://github.com/cbaltzer/UnitySpeakerFix
#import "ForceToSpeaker.h"
#import <AVFoundation/AVFoundation.h>


void Photon_IOSAudio_ForceToSpeaker() {
    // want audio to go to headset if it's connected
    if (Photon_IOSAudio_HeadsetConnected()) {
        return;
    }
    
    OSStatus error;
    UInt32 audioRouteOverride = kAudioSessionOverrideAudioRoute_Speaker;
    error = AudioSessionSetProperty(kAudioSessionProperty_OverrideAudioRoute,
                                     sizeof(audioRouteOverride),
                                     &audioRouteOverride);
    
    if (error) {
        NSLog(@"Audio already playing through speaker!");
    } else {
        NSLog(@"Forcing audio to speaker");
    }
}


bool Photon_IOSAudio_HeadsetConnected() {
    
    UInt32 routeSize = sizeof(CFStringRef);
    CFStringRef route = NULL;
    OSStatus error = AudioSessionGetProperty(kAudioSessionProperty_AudioRoute, &routeSize, &route);
    
    if (!error &&
        (route != NULL)&&
        ([(__bridge NSString*)route rangeOfString:@"Head"].location != NSNotFound))
    {
        /*  don't think this is needed
            see "the get rule":
            https://developer.apple.com/library/mac/#documentation/CoreFoundation/Conceptual/CFMemoryMgmt/Concepts/Ownership.html#//apple_ref/doc/uid/20001148-CJBEJBHH
        */
        //CFRelease(route);
        
        NSLog(@"Headset connected!");
        return true;
    }
    
    return false;
}
