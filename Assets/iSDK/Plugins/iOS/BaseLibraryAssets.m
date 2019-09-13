//
//  BaseLibraryAssets.m
//  YouLocal
//
//  Created by uBo on 24/11/2014.
//
//

@import Photos;
@import AssetsLibrary;

#import "BaseLibraryAssets.h"

@interface BaseLibraryAssets ()

@property (nonatomic, strong) ALAssetsLibrary *library;

@end

@implementation BaseLibraryAssets

@synthesize library = _library;

- (instancetype)init {
    if ( self = [super init] ) {
        _library = [[ALAssetsLibrary alloc] init];
    }
    
    return self;
}

- (NSString *)errorTextFromErrorCode:(NSInteger)code {
    NSString *errorText = @"";
    
    switch (code) {
        case ALAssetsLibraryWriteBusyError:
            errorText = NSLocalizedString(@"Please try again", @"Saving image to Library error");
            break;
            
        case ALAssetsLibraryWriteInvalidDataError:
        case ALAssetsLibraryWriteIncompatibleDataError:
        case ALAssetsLibraryWriteDataEncodingError:
        case ALAssetsLibraryDataUnavailableError:
            errorText = NSLocalizedString(@"Something went wrong, can not save the image", @"Saving image to Library error");
            break;
            
        case ALAssetsLibraryWriteDiskSpaceError:
            errorText = NSLocalizedString(@"No more space left", @"Saving image to Library error");
            break;
            
        case ALAssetsLibraryAccessUserDeniedError:
        case ALAssetsLibraryAccessGloballyDeniedError:
            errorText = NSLocalizedString(@"Can not save this photo, YouLocal does not have access to your photos. Please turn on Photos in your device Privacy settings.", @"Saving image to Library error");
            break;
            
        default:
            break;
    }
    
    return errorText;
}

- (void)saveImage:(UIImage *)image completionBlock:(void (^)(NSURL *assetURL))completionBlock errorBlock:(void(^)(void))errorBlock {
    [self.library writeImageToSavedPhotosAlbum:image.CGImage orientation:(ALAssetOrientation)image.imageOrientation completionBlock:^(NSURL *assetURL, NSError *error) {
        
        if ( error ) {
            NSString *errorText = [self errorTextFromErrorCode:error.code];
            
            if ( ![errorText isEqualToString:@""] ) {
                UIAlertView *alert = [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Photo", @"Saving image to Library alert view error title") message:errorText delegate:nil cancelButtonTitle:NSLocalizedString(@"OK", @"Saving image to Library alert view error button") otherButtonTitles:nil];
                
                [alert show];
            }
            
            if ( errorBlock ) {
                errorBlock();
            }
            
            return;
        }
        
        if ( completionBlock ) {
            completionBlock(assetURL);
        }
    }];
}

- (void)saveVideoURL:(NSURL *)url completionBlock:(void (^)(NSURL *assetURL))completionBlock errorBlock:(void(^)(void))errorBlock {
    [self.library writeVideoAtPathToSavedPhotosAlbum:url completionBlock:^(NSURL *assetURL, NSError *error)
    {
        if ( error ) {
            NSString *errorText = [self errorTextFromErrorCode:error.code];
            
            if ( ![errorText isEqualToString:@""] ) {
                UIAlertView *alert = [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"Media", @"Saving video to Library alert view error title") message:errorText delegate:nil cancelButtonTitle:NSLocalizedString(@"OK", @"Saving video to Library alert view error button") otherButtonTitles:nil];
                
                [alert show];
            }
            
            if ( errorBlock ) {
                errorBlock();
            }
            
            return;
        }
        
        if ( completionBlock ) {
            completionBlock(assetURL);
        }
    }];
}

@end
