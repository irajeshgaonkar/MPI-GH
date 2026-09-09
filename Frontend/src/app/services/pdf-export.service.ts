import { Injectable } from "@angular/core";
import jsPDF from "jspdf";
import html2canvas from "html2canvas";

interface PdfExportMetadataItem {
  label: string;
  value: string;
}

interface PdfExportOptions {
  fileName: string;
  orientation?: "portrait" | "landscape";
  title?: string;
  metadata?: PdfExportMetadataItem[];
}

@Injectable({
  providedIn: "root",
})
export class PdfExportService {
  async exportElement(element: HTMLElement, options: PdfExportOptions): Promise<void> {
    document.body.classList.add("pdf-exporting");

    try {
      const canvas = await html2canvas(element, {
        backgroundColor: "#ffffff",
        scale: 2,
        useCORS: true,
        logging: false,
        ignoreElements: (node) => node instanceof HTMLElement && node.hasAttribute("data-pdf-hide"),
        windowWidth: Math.max(element.scrollWidth, document.documentElement.clientWidth),
        windowHeight: Math.max(element.scrollHeight, document.documentElement.clientHeight),
      });

      const doc = new jsPDF({
        orientation: options.orientation || "landscape",
        unit: "pt",
        format: "letter",
      });

      const pageWidth = doc.internal.pageSize.getWidth();
      const pageHeight = doc.internal.pageSize.getHeight();
      const margin = 20;
      const contentWidth = pageWidth - (margin * 2);
      const metadataHeight = this.renderMetadata(doc, options, margin, contentWidth);
      const firstPageTop = margin + metadataHeight;
      const firstPageAvailableHeight = pageHeight - firstPageTop - margin;
      const laterPageAvailableHeight = pageHeight - (margin * 2);
      const firstPagePixels = Math.floor((firstPageAvailableHeight * canvas.width) / contentWidth);
      const laterPagePixels = Math.floor((laterPageAvailableHeight * canvas.width) / contentWidth);

      let offsetY = 0;
      let pageIndex = 0;

      while (offsetY < canvas.height) {
        const currentAvailableHeight = pageIndex === 0 ? firstPageAvailableHeight : laterPageAvailableHeight;
        const currentPixelsPerPage = pageIndex === 0 ? firstPagePixels : laterPagePixels;
        const topOffset = pageIndex === 0 ? firstPageTop : margin;
        const sliceHeight = Math.min(currentPixelsPerPage, canvas.height - offsetY);
        const pageCanvas = document.createElement("canvas");
        pageCanvas.width = canvas.width;
        pageCanvas.height = sliceHeight;

        const pageContext = pageCanvas.getContext("2d");
        if (!pageContext) {
          throw new Error("Unable to render PDF page content.");
        }

        pageContext.drawImage(
          canvas,
          0,
          offsetY,
          canvas.width,
          sliceHeight,
          0,
          0,
          canvas.width,
          sliceHeight,
        );

        if (pageIndex > 0) {
          doc.addPage();
        }

        const renderedHeight = Math.min((sliceHeight * contentWidth) / canvas.width, currentAvailableHeight);
        doc.addImage(pageCanvas.toDataURL("image/png"), "PNG", margin, topOffset, contentWidth, renderedHeight, undefined, "FAST");

        offsetY += sliceHeight;
        pageIndex += 1;
      }

      doc.save(options.fileName);
    } finally {
      document.body.classList.remove("pdf-exporting");
    }
  }

  private renderMetadata(doc: jsPDF, options: PdfExportOptions, margin: number, contentWidth: number): number {
    if (!options.title && (!options.metadata || !options.metadata.length)) {
      return 0;
    }

    const titleColor: [number, number, number] = [31, 59, 93];
    const textColor: [number, number, number] = [85, 112, 143];
    const borderColor: [number, number, number] = [215, 228, 242];
    let currentY = margin;

    if (options.title) {
      doc.setFont("helvetica", "normal");
      doc.setFontSize(28);
      doc.setTextColor(...titleColor);
      doc.text(options.title, margin, currentY + 22);
      currentY += 38;
    }

    if (options.metadata?.length) {
      doc.setFont("helvetica", "normal");
      doc.setFontSize(16);
      doc.setTextColor(...textColor);
      options.metadata.forEach((item) => {
        const lines = doc.splitTextToSize(`${item.label}: ${item.value}`, contentWidth);
        doc.text(lines, margin, currentY + 16);
        currentY += (lines.length * 18);
      });
      currentY += 6;
    }

    doc.setDrawColor(...borderColor);
    doc.setLineWidth(1);
    doc.line(margin, currentY, margin + contentWidth, currentY);

    return currentY - margin + 18;
  }
}
