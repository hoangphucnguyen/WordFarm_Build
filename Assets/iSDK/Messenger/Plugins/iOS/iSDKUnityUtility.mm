
#import "iSDKUnityUtility.h"

#include <string>
#import <Foundation/Foundation.h>

const char* const iSDK_OBJECT_NAME = "UnityiSDKPlugin";

static char* iSDKUnityMakeStringCopy (const char* string)
{
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

@implementation iSDKUnityUtility

+ (NSString *)stringFromCString:(const char *)string {
  if (string && string[0] != 0) {
    return [NSString stringWithUTF8String:string];
  }

  return nil;
}

+ (NSDictionary *)dictionaryFromKeys:(const char **)keys
                              values:(const char **)vals
                              length:(int)length
{
  NSMutableDictionary *params = nil;
  if(length > 0 && keys && vals) {
    params = [NSMutableDictionary dictionaryWithCapacity:length];
    for(int i = 0; i < length; i++) {
      if (vals[i] && vals[i] != 0 && keys[i] && keys[i] != 0) {
        params[[NSString stringWithUTF8String:keys[i]]] = [NSString stringWithUTF8String:vals[i]];
      }
    }
  }

  return params;
}

+ (void) sendCancelToUnity:(NSString *)unityMessage
                 requestId:(int)requestId
{
    [self sendMessageToUnity:unityMessage
                    userData:@{ @"cancelled" : @"true" }
                   requestId:requestId];
}

+ (void)sendErrorToUnity:(NSString *)unityMessage
                   error:(NSError *)error
               requestId:(int)requestId
{
    [self sendErrorToUnity:unityMessage
              errorMessage:error.localizedDescription
                 requestId:requestId];
}

+ (void)sendErrorToUnity:(NSString *)unityMessage
            errorMessage:(NSString *)errorMessage
               requestId:(int)requestId
{
    [self sendMessageToUnity:unityMessage
                    userData:@{ @"error" : errorMessage }
                   requestId:requestId];
}

+ (void)sendMessageToUnity:(NSString *)unityMessage
                  userData:(NSDictionary *)userData
                 requestId:(int)requestId
{
    NSMutableDictionary *resultDictionary = [ @{ @"callback_id": [@(requestId) stringValue] } mutableCopy];
    [resultDictionary addEntriesFromDictionary:userData];
    
    if (![NSJSONSerialization isValidJSONObject:resultDictionary]) {
        [self sendErrorToUnity:unityMessage errorMessage:@"Result cannot be converted to json" requestId:requestId];
        return;
    }
    
    NSError *serializationError = nil;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:resultDictionary options:0 error:&serializationError];
    if (serializationError) {
        [self sendErrorToUnity:unityMessage error:serializationError requestId:requestId];
        return;
    }
    
    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    if (!jsonString) {
        [self sendErrorToUnity:unityMessage errorMessage:@"Failed to generate response string" requestId:requestId];
        return;
    }
    
    const char *cString = [jsonString UTF8String];
    UnitySendMessage(iSDK_OBJECT_NAME, [unityMessage cStringUsingEncoding:NSASCIIStringEncoding], iSDKUnityMakeStringCopy(cString));
}

@end
