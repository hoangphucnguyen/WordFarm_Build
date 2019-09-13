//
//  HarpyPlugin.m
//  
//
//  Created by uBo on 01/04/2017.
//
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "Harpy/Harpy.h"
#include <string.h>
#include "DisplayManager.h"

#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

extern int _harpyInitialize(const char* iTunesAppName, const char* country) {
    UIWindow *window = [[[DisplayManager Instance] mainDisplay] window];
    
    [[Harpy sharedInstance] setAppName:GetStringParam(iTunesAppName)];
    [[Harpy sharedInstance] setPresentingViewController:window.rootViewController];
    [[Harpy sharedInstance] setAlertType:HarpyAlertTypeSkip];
    [[Harpy sharedInstance] setCountryCode:GetStringParam(country)];
    
    return 0;
}

extern int _harpyCheck() {
    [[Harpy sharedInstance] checkVersion];
    
    return 0;
}
