
@interface iSDKUnityUtility : NSObject
+ (NSDictionary *)dictionaryFromKeys:(const char **)keys values:(const char **)vals length:(int)length;
+ (NSString *)stringFromCString:(const char *)string;

+ (void)sendCancelToUnity:(NSString *)unityMessage requestId:(int)requestId;
+ (void)sendErrorToUnity:(NSString *)unityMessage error:(NSError *)error requestId:(int)requestId;
+ (void)sendErrorToUnity:(NSString *)unityMessage errorMessage:(NSString *)errorMessage requestId:(int)requestId;
+ (void)sendMessageToUnity:(NSString *)unityMessage userData:(NSDictionary *)userData requestId:(int)requestId;

@end
