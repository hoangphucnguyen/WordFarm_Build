
#import "iSDKUnityInterface.h"
#import "iSDKUnityMessenger.h"

static iSDKUnityInterface *_instance = [iSDKUnityInterface sharedInstance];

@interface iSDKUnityInterface ()

@property (nonatomic, strong) iSDKUnityMessenger *messenger;

@end

@implementation iSDKUnityInterface

+ (iSDKUnityInterface *)sharedInstance
{
    return _instance;
}

+ (void)initialize {
    if(!_instance) {
        _instance = [[iSDKUnityInterface alloc] init];
    }
}

- (id)init
{
    if(_instance != nil) {
        return _instance;
    }
    
    if ((self = [super init])) {
        _instance = self;
    }
    return self;
}

- (void)shareMessengerWithRequestId:(int)requestId
                         contentURL:(const char *)contentURL
                       contentTitle:(const char *)contentTitle
                 contentDescription:(const char *)contentDescription
                           photoURL:(const char *)photoURL{
    self.messenger = [[iSDKUnityMessenger alloc] init];
    
    [self.messenger shareMessengerWithRequestId:requestId
                                     contentURL:contentURL
                                   contentTitle:contentTitle
                             contentDescription:contentDescription
                                       photoURL:photoURL];
}

@end


#pragma mark - Actual Unity C# interface (extern C)

extern "C" {
    void IOSShareMessenger(int requestId,
                      const char *contentURL,
                      const char *contentTitle,
                      const char *contentDescription,
                      const char *photoURL)
    {
        [[iSDKUnityInterface sharedInstance] shareMessengerWithRequestId:requestId
                                                       contentURL:contentURL
                                                     contentTitle:contentTitle
                                               contentDescription:contentDescription
                                                         photoURL:photoURL];
    }
}
