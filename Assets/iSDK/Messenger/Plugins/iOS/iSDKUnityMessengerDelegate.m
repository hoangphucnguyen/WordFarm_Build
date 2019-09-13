
#import "iSDKUnityMessengerDelegate.h"

#import "iSDKUnityUtility.h"

#pragma mark - FBSDKSharingDelegate

@implementation iSDKUnityMessengerDelegate

- (void)sharer:(id<FBSDKSharing>)sharer didCompleteWithResults:(NSDictionary *)results
{
  if (results.count == 0) {
    // We no longer always send back a postId. In cases where the response is empty,
    // stuff in a didComplete so that Unity doesn't treat it as a malformed response.
    results = @{ @"didComplete" : @"1" };
  }
  [iSDKUnityUtility sendMessageToUnity:@"OnShareMessageComplete" userData:results requestId:[self requestId]];
  [self complete];
}

- (void)sharer:(id<FBSDKSharing>)sharer didFailWithError:(NSError *)error
{
  [iSDKUnityUtility sendErrorToUnity:@"OnShareMessageComplete" error:error requestId:[self requestId]];
  [self complete];
}

- (void)sharerDidCancel:(id<FBSDKSharing>)sharer
{
  [iSDKUnityUtility sendCancelToUnity:@"OnShareMessageComplete" requestId:[self requestId]];
  [self complete];
}

@end
