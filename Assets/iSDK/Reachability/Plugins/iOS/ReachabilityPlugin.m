//
//  HarpyPlugin.m
//  
//
//  Created by uBo on 01/04/2017.
//
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "Reachability.h"

#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

static Reachability *reach;

extern void _reachabilityInitialize(const char* domain) {
    reach = [Reachability reachabilityWithHostname:GetStringParam(domain)];
    
    [reach startNotifier];
}

extern bool _isReachable() {
    return [reach isReachable];
}
