
@interface iSDKUnityDelegate : NSObject
+ (instancetype)instanceWithRequestID:(int)requestID;
- (void)complete;
- (int)requestId;

@end
