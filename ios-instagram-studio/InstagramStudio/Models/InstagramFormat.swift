import CoreGraphics

enum InstagramFormat: String, CaseIterable, Identifiable {
    case squarePost
    case portraitPost
    case reel

    var id: String { rawValue }

    var title: String {
        switch self {
        case .squarePost:
            "Post 1:1"
        case .portraitPost:
            "Post 4:5"
        case .reel:
            "Reel 9:16"
        }
    }

    var pixelSize: CGSize {
        switch self {
        case .squarePost:
            CGSize(width: 1080, height: 1080)
        case .portraitPost:
            CGSize(width: 1080, height: 1350)
        case .reel:
            CGSize(width: 1080, height: 1920)
        }
    }

    var aspectRatio: CGFloat {
        pixelSize.width / pixelSize.height
    }
}
