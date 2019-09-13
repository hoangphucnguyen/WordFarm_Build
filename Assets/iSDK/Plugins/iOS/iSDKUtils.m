//
//  iSDKUtils.m
//  YouLocal
//
//
//

extern void _openSettings() {
    if (UIApplicationOpenSettingsURLString != NULL) {
        NSURL *appSettings = [NSURL URLWithString:UIApplicationOpenSettingsURLString];
        [[UIApplication sharedApplication] openURL:appSettings];
    }
}
