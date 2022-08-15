#import "IntegrateStoreKit.h"

@implementation IntegrateStoreKit
#if defined(__cplusplus)
extern "C"{
#endif
    void _goComment()
    {
        if([SKStoreReviewController respondsToSelector:@selector(requestReview)]) {// iOS 10.3 以上支持
            [SKStoreReviewController requestReview];
        } else { // iOS 10.3 之前的使用这个
/*             NSString *appId = @"1303467166";
            NSString  * nsStringToOpen = [NSString  stringWithFormat: @"itms-apps://itunes.apple.com/app/id%@?action=write-review",appId];//替换为对应的APPID
            [[UIApplication sharedApplication] openURL:[NSURL URLWithString:nsStringToOpen]]; */
        }
    }
#if defined(__cplusplus)
}
#endif

@end