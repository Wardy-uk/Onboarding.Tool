import SwiftUI
import UIKit
import AVFoundation

struct ImageRendererService {
    func renderImage(from sourceImage: UIImage, settings: RenderSettings) -> UIImage? {
        let size = settings.format.pixelSize
        let renderer = UIGraphicsImageRenderer(size: size)

        return renderer.image { context in
            let cg = context.cgContext

            drawBackground(in: cg, size: size)
            drawSourceImage(sourceImage, in: cg, canvasSize: size, scale: settings.imageScale, offset: settings.imageOffset)
            drawTextOverlay(in: cg, size: size, caption: settings.caption, hashtags: settings.hashtags)
        }
    }

    private func drawBackground(in context: CGContext, size: CGSize) {
        let colors = [
            UIColor(red: 0.20, green: 0.08, blue: 0.36, alpha: 1.0).cgColor,
            UIColor(red: 0.91, green: 0.32, blue: 0.58, alpha: 1.0).cgColor,
            UIColor(red: 0.99, green: 0.68, blue: 0.31, alpha: 1.0).cgColor
        ] as CFArray

        let locations: [CGFloat] = [0.0, 0.55, 1.0]
        guard let gradient = CGGradient(colorsSpace: CGColorSpaceCreateDeviceRGB(), colors: colors, locations: locations) else {
            return
        }

        context.drawLinearGradient(
            gradient,
            start: CGPoint(x: 0, y: 0),
            end: CGPoint(x: size.width, y: size.height),
            options: []
        )
    }

    private func drawSourceImage(_ image: UIImage, in context: CGContext, canvasSize: CGSize, scale: CGFloat, offset: CGSize) {
        let baseRect = AVMakeRect(aspectRatio: image.size, insideRect: CGRect(origin: .zero, size: canvasSize))
        let scaledSize = CGSize(width: baseRect.width * scale, height: baseRect.height * scale)

        let x = ((canvasSize.width - scaledSize.width) / 2.0) + offset.width
        let y = ((canvasSize.height - scaledSize.height) / 2.0) + offset.height

        let drawRect = CGRect(origin: CGPoint(x: x, y: y), size: scaledSize)
        image.draw(in: drawRect)

        context.setStrokeColor(UIColor.white.withAlphaComponent(0.22).cgColor)
        context.setLineWidth(6)
        context.stroke(CGRect(x: 0, y: 0, width: canvasSize.width, height: canvasSize.height))
    }

    private func drawTextOverlay(in context: CGContext, size: CGSize, caption: String, hashtags: String) {
        let margin: CGFloat = 48
        let overlayHeight: CGFloat = size.height * 0.24
        let overlayRect = CGRect(x: 0, y: size.height - overlayHeight, width: size.width, height: overlayHeight)

        let overlayPath = UIBezierPath(rect: overlayRect)
        UIColor.black.withAlphaComponent(0.35).setFill()
        overlayPath.fill()

        let paragraph = NSMutableParagraphStyle()
        paragraph.lineBreakMode = .byTruncatingTail

        let captionAttributes: [NSAttributedString.Key: Any] = [
            .font: UIFont.systemFont(ofSize: size.height * 0.04, weight: .semibold),
            .foregroundColor: UIColor.white,
            .paragraphStyle: paragraph
        ]

        let hashAttributes: [NSAttributedString.Key: Any] = [
            .font: UIFont.systemFont(ofSize: size.height * 0.03, weight: .regular),
            .foregroundColor: UIColor.white.withAlphaComponent(0.92),
            .paragraphStyle: paragraph
        ]

        (caption.isEmpty ? "Your caption" : caption)
            .draw(in: CGRect(x: margin, y: overlayRect.minY + 22, width: size.width - margin * 2, height: overlayHeight * 0.45), withAttributes: captionAttributes)

        (hashtags.isEmpty ? "#reels #post" : hashtags)
            .draw(in: CGRect(x: margin, y: overlayRect.minY + overlayHeight * 0.52, width: size.width - margin * 2, height: overlayHeight * 0.34), withAttributes: hashAttributes)
    }
}
