using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SHM.AppDomain.Constants;
using SHM.AppDomain.DTOs.OrdenPago;
using SHM.AppDomain.DTOs.OrdenPagoAprobacion;
using SHM.AppDomain.DTOs.OrdenPagoLiquidacion;

namespace SHM.AppWebHonorarioMedico.Reports;

/// <summary>
/// Documento PDF para Orden de Pago: encabezado, datos generales,
/// liquidaciones y detalle de comprobantes.
///
/// <author>ADG Antonio</author>
/// <created>2026-04-21</created>
/// </summary>
public class OrdenPagoDocument : IDocument
{
    private readonly OrdenPagoResponseDto _ordenPago;
    private readonly List<OrdenPagoLiquidacionResponseDto> _liquidaciones;
    private readonly List<DetalleLiquidacionItemDto> _detalle;
    private readonly List<OrdenPagoAprobacionResponseDto> _aprobaciones;
    private readonly string? _logoPath;

    private const string ColorPrimario    = "#3498db";
    private const string ColorAcento      = "#f26522";
    private const string ColorHeader      = "#6c757d";
    private const string ColorHeaderFila  = "#e9ecef";
    private const string ColorBorde       = "#dee2e6";
    private const string ColorTextoTenue  = "#6c757d";
    private const string ColorBlanco      = "#FFFFFF";
    private const string ColorVerde       = "#28a745";
    private const string ColorRojo        = "#dc3545";

    public OrdenPagoDocument(
        OrdenPagoResponseDto ordenPago,
        List<OrdenPagoLiquidacionResponseDto> liquidaciones,
        List<DetalleLiquidacionItemDto> detalle,
        List<OrdenPagoAprobacionResponseDto> aprobaciones,
        string? logoPath = null)
    {
        _ordenPago     = ordenPago;
        _liquidaciones = liquidaciones;
        _detalle       = detalle;
        _aprobaciones  = aprobaciones;
        _logoPath      = logoPath;
    }

    public DocumentMetadata GetMetadata()
    {
        var meta    = DocumentMetadata.Default;
        meta.Title  = $"Orden de Pago {_ordenPago.NumeroOrdenPago}";
        meta.Author = "SHM - Sistema de Honorarios Medicos";
        return meta;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.MarginHorizontal(1.2f, Unit.Centimetre);
            page.MarginVertical(1f, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(8).FontFamily(Fonts.Arial));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                if (!string.IsNullOrEmpty(_logoPath) && File.Exists(_logoPath))
                {
                    row.ConstantItem(100).Height(32).Image(_logoPath).FitArea();
                }
                else
                {
                    row.ConstantItem(100).Height(32).Background(ColorPrimario)
                       .AlignCenter().AlignMiddle()
                       .Text("SAN PABLO").FontColor(Colors.White).FontSize(10).Bold();
                }

                row.RelativeItem().PaddingLeft(10).Column(c =>
                {
                    c.Item().Text("SISTEMA DE HONORARIOS MÉDICOS")
                        .FontSize(11).Bold().FontColor(ColorPrimario);
                    c.Item().Text($"ORDEN DE PAGO  N° {_ordenPago.NumeroOrdenPago ?? "-"}")
                        .FontSize(14).Bold().FontColor(ColorAcento);
                    c.Item().Text($"Sede: {_ordenPago.NombreSede ?? "-"}")
                        .FontSize(8).FontColor(ColorTextoTenue);
                });

                row.ConstantItem(130).AlignRight().Column(c =>
                {
                    c.Item().Text($"Fecha: {_ordenPago.FechaGeneracion?.ToString("dd/MM/yyyy") ?? "-"}")
                        .FontSize(8);
                    c.Item().Text($"Estado: {EstadoDescripcion.OrdenPago.GetDescripcion(_ordenPago.Estado)}")
                        .FontSize(8).Bold().FontColor(ColorAcento);
                    c.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(7).FontColor(ColorTextoTenue);
                });
            });

            col.Item().PaddingTop(4).BorderBottom(1).BorderColor(ColorAcento);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(8).Column(col =>
        {
            col.Item().Element(ComposeResumen);
            col.Item().PaddingTop(10).Element(ComposeLiquidaciones);
            col.Item().PaddingTop(10).Element(ComposeDetalle);
            if (_aprobaciones.Count > 0)
                col.Item().PaddingTop(10).Element(ComposeAprobadores);
        });
    }

    private void ComposeResumen(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(c => SectionTitle(c, "Datos Generales"));

            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(3);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(3);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(3);
                });

                CeldaLabel(table, "Banco:");
                CeldaValor(table, _ordenPago.NombreBanco ?? "-");
                CeldaLabel(table, "Fecha Generación:");
                CeldaValor(table, _ordenPago.FechaGeneracion?.ToString("dd/MM/yyyy") ?? "-");
                CeldaLabel(table, "Estado:");
                CeldaValorBold(table, EstadoDescripcion.OrdenPago.GetDescripcion(_ordenPago.Estado));

                CeldaLabel(table, "N° Liquidaciones:");
                CeldaValor(table, _ordenPago.CantLiquidaciones?.ToString() ?? "-");
                CeldaLabel(table, "N° Comprobantes:");
                CeldaValor(table, _ordenPago.CantComprobantes?.ToString() ?? "-");
                CeldaLabel(table, "Sub Total S/:");
                CeldaValor(table, _ordenPago.MtoSubtotalAcum?.ToString("N2") ?? "--");

                CeldaLabel(table, "IGV S/:");
                CeldaValor(table, _ordenPago.MtoIgvAcum?.ToString("N2") ?? "--");
                CeldaLabel(table, "Imp. Renta S/:");
                CeldaValor(table, _ordenPago.MtoRentaAcum?.ToString("N2") ?? "--");
                CeldaLabel(table, "TOTAL S/:");
                CeldaValorDestacado(table, _ordenPago.MtoTotalAcum?.ToString("N2") ?? "--");

                if (!string.IsNullOrWhiteSpace(_ordenPago.Comentarios))
                {
                    CeldaLabel(table, "Comentarios:");
                    table.Cell().ColumnSpan(5).Padding(3).Background(ColorHeaderFila)
                         .Text(_ordenPago.Comentarios).FontSize(8);
                }
            });
        });
    }

    private void ComposeLiquidaciones(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(c => SectionTitle(c, $"Liquidaciones ({_liquidaciones.Count})"));

            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(20);
                    cols.RelativeColumn(3);
                    cols.RelativeColumn(3);
                    cols.RelativeColumn(4);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(4);
                    cols.ConstantColumn(40);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(2);
                });

                var hdrs = new[] { "#", "Cod. Liquidación", "Tipo", "Descripción", "Periodo", "Banco", "Compr.", "Sub Total S/.", "IGV S/.", "Imp. Renta S/.", "Total S/." };
                foreach (var h in hdrs)
                    EncabezadoCelda(table, h);

                int idx = 1;
                foreach (var liq in _liquidaciones)
                {
                    var bg = idx % 2 == 0 ? ColorHeaderFila : ColorBlanco;
                    FilaCelda(table, idx.ToString(), bg, center: true);
                    FilaCelda(table, liq.NumeroLiquidacion ?? "-", bg);
                    FilaCelda(table, liq.DesTipoLiquidacion ?? liq.TipoLiquidacion ?? "-", bg);
                    FilaCelda(table, liq.DescripcionLiquidacion ?? "-", bg);
                    FilaCelda(table, liq.PeriodoLiquidacion ?? "-", bg, center: true);
                    FilaCelda(table, liq.NombreBanco ?? "-", bg);
                    FilaCelda(table, liq.CantComprobantes?.ToString() ?? "-", bg, center: true);
                    FilaCeldaMonto(table, liq.MtoSubtotalAcum, bg);
                    FilaCeldaMonto(table, liq.MtoIgvAcum, bg);
                    FilaCeldaMonto(table, liq.MtoRentaAcum, bg);
                    FilaCeldaMonto(table, liq.MtoTotalAcum, bg, bold: true);
                    idx++;
                }

                table.Cell().ColumnSpan(7).Background(ColorHeaderFila).Padding(3)
                     .Text("TOTAL:").Bold().FontSize(8).AlignRight();
                FilaCeldaMonto(table, _liquidaciones.Sum(l => l.MtoSubtotalAcum ?? 0), ColorHeaderFila, bold: true);
                FilaCeldaMonto(table, _liquidaciones.Sum(l => l.MtoIgvAcum ?? 0), ColorHeaderFila, bold: true);
                FilaCeldaMonto(table, _liquidaciones.Sum(l => l.MtoRentaAcum ?? 0), ColorHeaderFila, bold: true);
                FilaCeldaMonto(table, _liquidaciones.Sum(l => l.MtoTotalAcum ?? 0), ColorHeaderFila, bold: true);
            });
        });
    }

    private void ComposeDetalle(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(c => SectionTitle(c, $"Detalle de Comprobantes ({_detalle.Count})"));

            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(20);
                    cols.RelativeColumn(3);
                    cols.ConstantColumn(60);
                    cols.ConstantColumn(60);
                    cols.RelativeColumn(4);
                    cols.RelativeColumn(3);
                    cols.RelativeColumn(2);
                    cols.ConstantColumn(55);
                    cols.RelativeColumn(3);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(2);
                });

                var hdrs = new[] { "#", "Liquidación", "RUC", "Cod. Acreedor", "Cía Médica", "Banco", "Comprobante", "F. Emisión", "Estado", "Sub Total S/.", "IGV S/.", "Imp. Renta S/.", "Total S/." };
                foreach (var h in hdrs)
                    EncabezadoCelda(table, h);

                int idx = 1;
                foreach (var det in _detalle)
                {
                    var bg = idx % 2 == 0 ? ColorHeaderFila : ColorBlanco;
                    var numComp = (int.TryParse(det.Numero, out int nParsed) ? nParsed : 0).ToString("D7");
                    var comprobante = !string.IsNullOrEmpty(det.TipoComprobante) && !string.IsNullOrEmpty(det.Serie) && !string.IsNullOrEmpty(det.Numero)
                        ? $"{det.TipoComprobante.PadLeft(2, '0')}-0{det.Serie}-{numComp}" : "-";

                    FilaCelda(table, idx.ToString(), bg, center: true);
                    FilaCelda(table, det.NumeroLiquidacion ?? "-", bg);
                    FilaCelda(table, det.Ruc ?? "-", bg, center: true);
                    FilaCelda(table, det.CodigoAcreedor ?? "-", bg, center: true);
                    FilaCelda(table, det.RazonSocial ?? "-", bg);
                    FilaCelda(table, det.NombreBanco ?? "-", bg);
                    FilaCelda(table, comprobante, bg, center: true);
                    FilaCelda(table, det.FechaEmision?.ToString("dd/MM/yyyy") ?? "-", bg, center: true);
                    FilaCelda(table, EstadoDescripcion.Produccion.GetDescripcion(det.Estado), bg, center: true);
                    FilaCeldaMonto(table, det.MtoSubtotal, bg);
                    FilaCeldaMonto(table, det.MtoIgv, bg);
                    FilaCeldaMonto(table, det.MtoRenta, bg);
                    FilaCeldaMonto(table, det.MtoTotal, bg, bold: true);
                    idx++;
                }

                table.Cell().ColumnSpan(9).Background(ColorHeaderFila).Padding(3)
                     .Text("TOTAL GENERAL:").Bold().FontSize(8).AlignRight();
                FilaCeldaMonto(table, _detalle.Sum(d => d.MtoSubtotal ?? 0), ColorHeaderFila, bold: true);
                FilaCeldaMonto(table, _detalle.Sum(d => d.MtoIgv ?? 0), ColorHeaderFila, bold: true);
                FilaCeldaMonto(table, _detalle.Sum(d => d.MtoRenta ?? 0), ColorHeaderFila, bold: true);
                FilaCeldaMonto(table, _detalle.Sum(d => d.MtoTotal ?? 0), ColorHeaderFila, bold: true);
            });
        });
    }

    private void ComposeAprobadores(IContainer container)
    {
        var ordenados = _aprobaciones.OrderBy(a => a.Orden ?? 99).ToList();

        container.Column(col =>
        {
            col.Item().Element(c => SectionTitle(c, "Aprobadores"));

            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(25);   // #
                    cols.RelativeColumn(4);    // Perfil
                    cols.RelativeColumn(5);    // Nombre aprobador
                    cols.RelativeColumn(3);    // Estado
                    cols.RelativeColumn(3);    // Fecha y hora
                });

                var hdrs = new[] { "#", "Perfil de Aprobación", "Aprobador", "Estado", "Fecha y Hora" };
                foreach (var h in hdrs)
                    EncabezadoCelda(table, h);

                int idx = 1;
                foreach (var apr in ordenados)
                {
                    var bg        = idx % 2 == 0 ? ColorHeaderFila : ColorBlanco;
                    var esAprobado = apr.Estado == "APROBADO";
                    var color     = esAprobado ? ColorVerde : (apr.Estado == "RECHAZADO" ? ColorRojo : ColorTextoTenue);
                    var estadoTxt = esAprobado ? "Aprobado" : (apr.Estado == "RECHAZADO" ? "Rechazado" : (apr.Estado ?? "-"));
                    var fechaTxt  = apr.FechaAprobacion.HasValue
                        ? apr.FechaAprobacion.Value.ToString("dd/MM/yyyy HH:mm")
                        : "-";

                    FilaCelda(table, idx.ToString(), bg, center: true);
                    FilaCelda(table, apr.NombrePerfil ?? "-", bg);
                    FilaCeldaBold(table, apr.NombreAprobador ?? "-", bg);
                    FilaCeldaColor(table, estadoTxt, bg, color);
                    FilaCelda(table, fechaTxt, bg, center: true);
                    idx++;
                }
            });
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(ColorBorde).PaddingTop(4).Row(row =>
        {
            row.RelativeItem().Text(t =>
            {
                t.DefaultTextStyle(s => s.FontSize(7).FontColor(ColorTextoTenue));
                t.Span("Sistema de Honorarios Médicos - Grupo San Pablo  |  ");
                t.Span($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
            });

            row.RelativeItem().AlignRight().Text(t =>
            {
                t.DefaultTextStyle(s => s.FontSize(7).FontColor(ColorTextoTenue));
                t.Span("Página ");
                t.CurrentPageNumber();
                t.Span(" de ");
                t.TotalPages();
            });
        });
    }

    // -------------------------------------------------------------------------
    // Helpers de renderizado
    // -------------------------------------------------------------------------

    private static void SectionTitle(IContainer container, string title)
    {
        container.Background(ColorHeader).Padding(4)
                 .Text(title).FontColor(Colors.White).FontSize(9).Bold();
    }

    private static void EncabezadoCelda(TableDescriptor table, string label)
    {
        table.Cell().Background(ColorHeader).Padding(3)
             .Text(label).FontColor(Colors.White).FontSize(7).Bold().AlignCenter();
    }

    private static void CeldaLabel(TableDescriptor table, string label)
    {
        table.Cell().Background(ColorHeaderFila).Padding(3)
             .Text(label).FontSize(8).FontColor(ColorTextoTenue);
    }

    private static void CeldaValor(TableDescriptor table, string valor)
    {
        table.Cell().Padding(3).Text(valor).FontSize(8);
    }

    private static void CeldaValorBold(TableDescriptor table, string valor)
    {
        table.Cell().Padding(3).Text(valor).FontSize(8).Bold();
    }

    private static void CeldaValorDestacado(TableDescriptor table, string valor)
    {
        table.Cell().Padding(3).Text(valor).FontSize(8).Bold().FontColor(ColorAcento);
    }

    private static void FilaCelda(TableDescriptor table, string valor, string bg, bool center = false)
    {
        var cell = table.Cell().Background(bg).BorderBottom(1).BorderColor(ColorBorde).Padding(3);
        var txt  = cell.Text(valor).FontSize(7);
        if (center) txt.AlignCenter();
    }

    private static void FilaCeldaMonto(TableDescriptor table, decimal? valor, string bg, bool bold = false)
    {
        var cell = table.Cell().Background(bg).BorderBottom(1).BorderColor(ColorBorde).Padding(3);
        var txt  = cell.Text(valor?.ToString("N2") ?? "--").FontSize(7).AlignRight();
        if (bold) txt.Bold();
    }

    private static void FilaCeldaBold(TableDescriptor table, string valor, string bg)
    {
        table.Cell().Background(bg).BorderBottom(1).BorderColor(ColorBorde).Padding(3)
             .Text(valor).FontSize(7).Bold();
    }

    private static void FilaCeldaColor(TableDescriptor table, string valor, string bg, string color)
    {
        table.Cell().Background(bg).BorderBottom(1).BorderColor(ColorBorde).Padding(3)
             .Text(valor).FontSize(7).Bold().FontColor(color).AlignCenter();
    }
}
