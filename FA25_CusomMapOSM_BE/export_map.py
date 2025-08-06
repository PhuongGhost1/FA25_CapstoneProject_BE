import sys
import os
from qgis.core import (
    QgsApplication,
    QgsProject,
    QgsLayoutExporter,
    QgsLayoutItemLabel,
    QgsCoordinateReferenceSystem,
    QgsCoordinateTransform,
    QgsRectangle
)
from PyQt5.QtGui import QFont
import traceback

print("🚀 Script started")

# Argument check
if len(sys.argv) != 8:
    print("Usage: python export_map.py <project.qgz> <output_file> <format> <xmin> <ymin> <xmax> <ymax>")
    sys.exit(1)

# Read arguments
project_path = sys.argv[1]
output_path = sys.argv[2]
export_format = sys.argv[3].upper()
xmin, ymin, xmax, ymax = map(float, sys.argv[4:])

print("🚀 Starting export script...")
print(f"📂 Project: {project_path}")
print(f"📸 Output: {output_path}")
print(f"📐 Input Bounds (EPSG:4326): {xmin}, {ymin}, {xmax}, {ymax}")
print(f"🧾 Format: {export_format}")

# Initialize QGIS
qgs = QgsApplication([], False)
qgs.initQgis()

try:
    version = getattr(QgsApplication, "qgisVersion", None)
    if callable(version):
        print("🔍 QGIS version:", version())
    else:
        print("⚠️ Unable to detect QGIS version.")

    project = QgsProject.instance()
    if not project.read(project_path):
        raise Exception(f"❌ Failed to read project file: {project_path}")

    layout_manager = project.layoutManager()
    layouts = layout_manager.layouts()
    if not layouts:
        raise Exception("❌ No layout found in the QGIS project.")
    layout = layouts[0]

    # Transform extent
    project_crs = project.crs()
    transform = QgsCoordinateTransform(QgsCoordinateReferenceSystem("EPSG:4326"), project_crs, project)
    extent = QgsRectangle(xmin, ymin, xmax, ymax)
    transformed_extent = transform.transformBoundingBox(extent)
    print(f"📐 Transformed extent: {transformed_extent.toString()}")

    # Set extent
    map_items = [item for item in layout.items() if hasattr(item, "setExtent")]
    if map_items:
        map_item = map_items[0]
        map_item.setExtent(transformed_extent)
        map_item.refresh()
        print("🌀 Layout refreshed and ready for export.")
    else:
        print("⚠️ No map item found in the layout.")

    # Set label fonts
    for item in layout.items():
        if isinstance(item, QgsLayoutItemLabel):
            font = QFont("DejaVu Sans", item.textFormat().font().pointSize())
            fmt = item.textFormat()
            fmt.setFont(font)
            item.setTextFormat(fmt)
    print("🔤 Labels updated.")

    # Export
    exporter = QgsLayoutExporter(layout)
    if export_format == "PDF":
        result = exporter.exportToPdf(output_path, QgsLayoutExporter.PdfExportSettings())
    elif export_format == "IMAGE":
        from PyQt5.QtCore import QSize
        settings = QgsLayoutExporter.ImageExportSettings()
        settings.dpi = 300
        result = exporter.exportToImage(output_path, settings)
    else:
        print(f"❌ Unsupported format: {export_format}")
        sys.exit(1)

    if result == QgsLayoutExporter.Success:
        print(f"✅ Export successful: {output_path}")
    else:
        print(f"❌ Export failed with result code: {result}")

except Exception as e:
    print("❌ Exception occurred:", str(e))
    traceback.print_exc()

finally:
    qgs.exitQgis()
    print("👋 QGIS exited")