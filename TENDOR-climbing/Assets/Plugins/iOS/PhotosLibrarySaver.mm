#import <Foundation/Foundation.h>
#import <Photos/Photos.h>
#import <UIKit/UIKit.h>

extern "C" {
    void _SaveVideoToPhotosLibrary(const char* filePath) {
        NSString *videoPath = [NSString stringWithUTF8String:filePath];
        NSURL *videoURL = [NSURL fileURLWithPath:videoPath];
        
        // Check if file exists
        if (![[NSFileManager defaultManager] fileExistsAtPath:videoPath]) {
            NSLog(@"[PhotosLibrarySaver] Video file not found: %@", videoPath);
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
    }
} 