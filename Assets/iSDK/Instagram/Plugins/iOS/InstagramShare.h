//
//  InstagramShare.h
//  Unity-iPhone
//
//  Created by uBo on 26/10/2016.
//
//

#import <Foundation/Foundation.h>

void _handshake();
void IOSShareInstagram(const char *, const char *);

@interface InstagramShare : NSObject <UIDocumentInteractionControllerDelegate>
{
    UIWindow *nativeWindow;
}

@property (nonatomic, strong) UIDocumentInteractionController *dic;

+ (instancetype)sharedInstance;

- (void)handshake;
- (void)postToInstagram:(NSString *)message WithImage:(NSString *)imagePath;

@end
