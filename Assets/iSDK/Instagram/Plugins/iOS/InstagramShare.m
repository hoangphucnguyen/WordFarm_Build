//
//  InstagramShare.m
//  Unity-iPhone
//
//  Created by uBo on 26/10/2016.
//
//

#import "InstagramShare.h"
#import "SVProgressHUD.h"
#import "BaseLibraryAssets.h"

@import Photos;
@import AssetsLibrary;

void IOSShareInstagramHandshake()
{
    [[InstagramShare sharedInstance] handshake];
}

void IOSShareInstagram(const char * message, const char * imagePath)
{
    NSString *m = nil;
    if ( message ) {
        m = [NSString stringWithUTF8String:message];
    }
    
    NSString *i = [NSString stringWithUTF8String:imagePath];
    
    [[InstagramShare sharedInstance] postToInstagram:m WithImage:i];
}

@implementation InstagramShare

static InstagramShare *sharedInstance = nil;
static dispatch_once_t onceToken;

+ (instancetype)sharedInstance {
    dispatch_once(&onceToken, ^{
        sharedInstance = [[InstagramShare alloc] init];
    });
    
    return sharedInstance;
}


-(id)init
{
    if (self = [super init])
    {
        nativeWindow = [UIApplication sharedApplication].keyWindow;
    }
    
    return self;
}

- (void)handshake
{
    NSLog(@"Handshake completed!");
}

- (void)postToInstagram:(NSString *)message WithImage:(NSString *)imagePath;
{
    NSURL *appURL = [NSURL URLWithString:@"instagram://app"];
    
    if([[UIApplication sharedApplication] canOpenURL:appURL]) {
        if ( ![[NSFileManager defaultManager] fileExistsAtPath:imagePath] ) {
            [SVProgressHUD showErrorWithStatus:NSLocalizedString(@"Image file does not exists. Try again later", @"Екран 'Invite';") duration:3];
            return;
        }
        
        // Image
        UIImage *image = [UIImage imageWithContentsOfFile:imagePath];
        
        BaseLibraryAssets *library = [[BaseLibraryAssets alloc] init];
        
        PHAsset __block *asset;
        
        [library saveImage:image completionBlock:^(NSURL *assetURL) {
            PHFetchResult <PHAsset *> *assets = [PHAsset fetchAssetsWithALAssetURLs:@[assetURL] options:0];
            asset = [assets firstObject];
            
            if ( asset ) {
                NSURL *instagramURL = [NSURL URLWithString:[NSString stringWithFormat:@"instagram://library?LocalIdentifier=%@", asset.localIdentifier]];
                
                if ([[UIApplication sharedApplication] canOpenURL:instagramURL]) {
                    [[UIApplication sharedApplication] openURL:instagramURL];
                }
            }
        } errorBlock:^{
            
        }];
    }
    else
    {
        [SVProgressHUD showErrorWithStatus:NSLocalizedString(@"Please install Instagram to be able to share", @"Екран 'Invite';") duration:3];
    }
}

@end
