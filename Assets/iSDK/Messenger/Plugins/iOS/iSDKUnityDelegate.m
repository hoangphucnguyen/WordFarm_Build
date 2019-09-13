#import "iSDKUnityDelegate.h"

static NSMutableArray *g_instances;

@implementation iSDKUnityDelegate {
  int _requestID;
}

+ (void)initialize
{
  if (self == [self class]) {
    g_instances = [NSMutableArray array];
  }
}

+ (instancetype)instanceWithRequestID:(int)requestID
{
  iSDKUnityDelegate *instance = [[[self class] alloc] init];
  instance->_requestID = requestID;
  [g_instances addObject:instance];
  return instance;
}

#pragma mark - Private helpers

- (void)complete
{
  [g_instances removeObject:self];
}

- (int)requestId {
    return _requestID;
}

@end
