#import <Foundation/Foundation.h>
#if __has_include(<Photos/Photos.h>)
#import <Photos/Photos.h>
#define PHOTOS_AVAILABLE 1
#else
#define PHOTOS_AVAILABLE 0
#endif
#import <UIKit/UIKit.h>

extern "C" {
    void _SaveVideoToPhotosLibrary(const char* filePath) {
#if PHOTOS_AVAILABLE
        NSString *videoPath = [NSString stringWithUTF8String:filePath];
        NSURL *videoURL = [NSURL fileURLWithPath:videoPath];
        
        // Check if file exists
        if (![[NSFileManager defaultManager] fileExistsAtPath:videoPath]) {
            NSLog(@"[PhotosLibrarySaver] Video file not found: %@", videoPath);
            return;
        }
        
        // Check if Photos framework classes are available at runtime
        if (NSClassFromString(@"PHPhotoLibrary") == nil) {
            NSLog(@"[PhotosLibrarySaver] Photos framework not available at runtime");
            return;
        }
        
        // Request Photos library permission and save video
        [PHPhotoLibrary requestAuthorizationForAccessLevel:PHAccessLevelAddOnly handler:^(PHAuthorizationStatus status) {
            if (status == PHAuthorizationStatusAuthorized || status == PHAuthorizationStatusLimited) {
                [[PHPhotoLibrary sharedPhotoLibrary] performChanges:^{
                    [PHAssetChangeRequest creationRequestForAssetFromVideoAtFileURL:videoURL];
                } completionHandler:^(BOOL success, NSError * _Nullable error) {
                    dispatch_async(dispatch_get_main_queue(), ^{
                        if (success) {
                            NSLog(@"[PhotosLibrarySaver] ✅ Video saved to Photos library: %@", [videoPath lastPathComponent]);
                        } else {
                            NSLog(@"[PhotosLibrarySaver] ❌ Failed to save video to Photos library: %@", error.localizedDescription);
                        }
                    });
                }];
            } else {
                NSLog(@"[PhotosLibrarySaver] ❌ Photos library access denied. Status: %ld", (long)status);
            }
        }];
#else
        NSLog(@"[PhotosLibrarySaver] Photos framework not available at compile time");
#endif
    }
} 