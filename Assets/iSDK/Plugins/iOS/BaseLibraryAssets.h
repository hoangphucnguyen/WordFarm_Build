//
//  BaseLibraryAssets.h
//  YouLocal
//
//  Created by uBo on 24/11/2014.
//
//

#import <Foundation/Foundation.h>

@interface BaseLibraryAssets : NSObject

- (void)saveImage:(UIImage *)image completionBlock:(void (^)(NSURL *assetURL))completionBlock errorBlock:(void(^)(void))errorBlock;
- (void)saveVideoURL:(NSURL *)url completionBlock:(void (^)(NSURL *assetURL))completionBlock errorBlock:(void(^)(void))errorBlock;

@end
