
#import "iSDKUnityMessenger.h"
#import "iSDKUnityUtility.h"
#import "iSDKUnityMessengerDelegate.h"
#import <Foundation/Foundation.h>

#import <FBSDKCoreKit/FBSDKCoreKit.h>
#import <FBSDKLoginKit/FBSDKLoginKit.h>
#import <FBSDKShareKit/FBSDKShareKit.h>

@implementation iSDKUnityMessenger

- (void)shareMessengerWithRequestId:(int)requestId
                    contentURL:(const char *)contentURL
                  contentTitle:(const char *)contentTitle
            contentDescription:(const char *)contentDescription
                      photoURL:(const char *)photoURL
{
    FBSDKShareLinkContent *linkContent = [[FBSDKShareLinkContent alloc] init];
    
    NSString *contentUrlStr = [iSDKUnityUtility stringFromCString:contentURL];
    if (contentUrlStr) {
        linkContent.contentURL = [NSURL URLWithString:contentUrlStr];
    }
    
    NSString *contentTitleStr = [iSDKUnityUtility stringFromCString:contentTitle];
    if (contentTitleStr) {
        linkContent.contentTitle = contentTitleStr;
    }
    
    NSString *contentDescStr = [iSDKUnityUtility stringFromCString:contentDescription];
    if (contentDescStr) {
        linkContent.contentDescription = contentDescStr;
    }
    
    NSString *imageURL = [iSDKUnityUtility stringFromCString:photoURL];
    if (imageURL) {
        linkContent.imageURL = [NSURL URLWithString:imageURL];
    }
    
    [self shareMessengerWithRequestId:requestId
                       shareContent:linkContent];
}

- (void)shareMessengerWithRequestId:(int)requestId
                     shareContent:(FBSDKShareLinkContent *)linkContent
{
    FBSDKMessageDialog *dialog = [[FBSDKMessageDialog alloc] init];
    dialog.shareContent = linkContent;
    iSDKUnityMessengerDelegate *delegate = [iSDKUnityMessengerDelegate instanceWithRequestID:requestId];
    dialog.delegate = delegate;
    
    NSError *error;
    if (![dialog validateWithError:&error]) {
        [iSDKUnityUtility sendErrorToUnity:@"OnShareMessageComplete" error:error requestId:requestId];
    }
    if (![dialog show]) {
        [iSDKUnityUtility sendErrorToUnity:@"OnShareMessageComplete" errorMessage:@"Failed to show share dialog" requestId:requestId];
    }
}

@end
