
@interface iSDKUnityMessenger : NSObject

- (void)shareMessengerWithRequestId:(int)requestId
                         contentURL:(const char *)contentURL
                       contentTitle:(const char *)contentTitle
                 contentDescription:(const char *)contentDescription
                           photoURL:(const char *)photoURL;

@end
