import UIKit
import React
import React_RCTAppDelegate

@main
class AppDelegate: RCTAppDelegate {
    override func application(
        _ application: UIApplication,
        didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]? = nil
    ) -> Bool {
        self.moduleName = "Spendly"
        return super.application(application, didFinishLaunchingWithOptions: launchOptions)
    }

    override func sourceURL(for bridge: RCTBridge!) -> URL! {
        return self.bundleURL()
    }

    func bundleURL() -> URL! {
#if DEBUG
        return RCTBundleURLProvider.jsBundleURL(
            forBundleRoot: "index",
            packagerHost: "localhost",
            enableDev: true,
            enableMinification: false,
            inlineSourceMap: false
        )
#else
        return Bundle.main.url(forResource: "main", withExtension: "jsbundle")
#endif
    }
}
