﻿using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternNoteViewModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.PDFTemplates
{
    public class GarmentInternNotePDFTemplate
    {
        public MemoryStream GeneratePdfTemplate(GarmentInternNoteViewModel viewModel, int clientTimeZoneOffset, IGarmentDeliveryOrderFacade DOfacade)
        {
            Font header_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 18);
            Font normal_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 9);
            Font bold_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);
            //Font header_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);

            Document document = new Document(PageSize.A4, 40, 40, 40, 40);
            document.AddHeader("Header", viewModel.inNo);
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            writer.PageEvent = new PDFPages();
            document.Open();

            Chunk chkHeader = new Chunk(" ");
            Phrase pheader = new Phrase(chkHeader);
            HeaderFooter header = new HeaderFooter(pheader, false);
            header.Border = Rectangle.NO_BORDER;
            header.Alignment = Element.ALIGN_RIGHT;
            document.Header = header;

            #region Header

            string addressString = "PT DAN LIRIS" + "\n" + "Head Office: Kelurahan Banaran" + "\n" + "Kecamatan Grogol" + "\n" + "Sukoharjo 57193 - INDONESIA" + "\n" + "PO.BOX 166 Solo 57100" + "\n" + "Telp. (0271) 740888, 714400" + "\n" + "Fax. (0271) 735222, 740777";
            Paragraph address = new Paragraph(addressString, bold_font) { Alignment = Element.ALIGN_LEFT };
            document.Add(address);
            bold_font.SetStyle(Font.NORMAL);

            string titleString = "NOTA INTERN\n\n";
            Paragraph title = new Paragraph(titleString, bold_font) { Alignment = Element.ALIGN_CENTER };
            document.Add(title);
            bold_font.SetStyle(Font.NORMAL);

            PdfPTable tableInternNoteHeader = new PdfPTable(2);
            tableInternNoteHeader.SetWidths(new float[] { 4.5f, 4.5f });
            PdfPCell cellInternNoteHeaderLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell cellInternNoteHeaderRight = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            
                cellInternNoteHeaderLeft.Phrase = new Phrase("No. Nota Intern" + "      : " + viewModel.inNo, normal_font);
                tableInternNoteHeader.AddCell(cellInternNoteHeaderLeft);

                cellInternNoteHeaderRight.Phrase = new Phrase("Tanggal Nota Intern" + "       : " + viewModel.inDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("dd MMMM yyyy", new CultureInfo("id-ID")), normal_font);
                tableInternNoteHeader.AddCell(cellInternNoteHeaderRight);

                cellInternNoteHeaderLeft.Phrase = new Phrase("Kode Supplier" + "        : " + viewModel.supplier.Code, normal_font);
                tableInternNoteHeader.AddCell(cellInternNoteHeaderLeft);
            
                DateTimeOffset paymentduedates;
                string paymentmethods = "";

                var paymentDueDateTemp = DateTimeOffset.MinValue;
                foreach (GarmentInternNoteItemViewModel item in viewModel.items)
                {
                    foreach (GarmentInternNoteDetailViewModel detail in item.details)
                    {
                        if (paymentDueDateTemp > detail.paymentDueDate)
                        {
                            paymentduedates = paymentDueDateTemp;
                        }
                        else if(detail.paymentDueDate > paymentDueDateTemp)
                        {
                            paymentduedates = detail.paymentDueDate;
                        }
                        paymentmethods = detail.deliveryOrder.paymentMethod;
                    }

                }

                cellInternNoteHeaderRight.Phrase = new Phrase("Tanggal Jatuh Tempo" + "    : " + paymentduedates.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("dd MMMM yyyy", new CultureInfo("id-ID")), normal_font);
                tableInternNoteHeader.AddCell(cellInternNoteHeaderRight);

                cellInternNoteHeaderLeft.Phrase = new Phrase("Nama Supplier" + "       : " + viewModel.supplier.Name, normal_font);
                tableInternNoteHeader.AddCell(cellInternNoteHeaderLeft);

                cellInternNoteHeaderRight.Phrase = new Phrase("Term Pembayaran" + "         : " + paymentmethods, normal_font);
                tableInternNoteHeader.AddCell(cellInternNoteHeaderRight);


            PdfPCell cellInternNoteHeader = new PdfPCell(tableInternNoteHeader); // dont remove
            tableInternNoteHeader.ExtendLastRow = false;
            tableInternNoteHeader.SpacingAfter = 10f;
            document.Add(tableInternNoteHeader);
            #endregion
            
            #region Table_Of_Content
            PdfPCell cellCenter = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
            PdfPCell cellRight = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
            PdfPCell cellLeft = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };

            PdfPTable tableContent = new PdfPTable(8);
            tableContent.SetWidths(new float[] { 3.5f, 4f, 5f, 5.5f, 3f, 3f, 3f,4f });
                cellCenter.Phrase = new Phrase("NO. Surat Jalan", bold_font);
                tableContent.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase("Tgl. Surat Jalan", bold_font);
                tableContent.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase("No. Referensi PR", bold_font);
                tableContent.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase("Keterangan Barang", bold_font);
                tableContent.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase("Jumlah", bold_font);
                tableContent.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase("Satuan", bold_font);
                tableContent.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase("Harga Satuan", bold_font);
                tableContent.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase("Harga Total", bold_font);
                tableContent.AddCell(cellCenter);

            double totalPriceTotal = 0;
            double total = 0;
            double ppn = 0;
            double pph = 0;
            double maxtotal = 0;
            Dictionary<string, double> units = new Dictionary<string, double>();
            foreach (GarmentInternNoteItemViewModel item in viewModel.items)
            {
                foreach (GarmentInternNoteDetailViewModel detail in item.details)
                {
                    cellLeft.Phrase = new Phrase(detail.deliveryOrder.doNo, normal_font);
                    tableContent.AddCell(cellLeft);

                    string doDate = detail.deliveryOrder.doDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("dd MMMM yyyy", new CultureInfo("id-ID"));

                    cellLeft.Phrase = new Phrase(doDate, normal_font);
                    tableContent.AddCell(cellLeft);

                    cellLeft.Phrase = new Phrase(detail.poSerialNumber, normal_font);
                    tableContent.AddCell(cellLeft);

                    cellLeft.Phrase = new Phrase(detail.product.Name, normal_font);
                    tableContent.AddCell(cellLeft);

                    cellRight.Phrase = new Phrase(detail.quantity.ToString("N", new CultureInfo("id-ID")), normal_font);
                    tableContent.AddCell(cellRight);

                    cellRight.Phrase = new Phrase(detail.uomUnit.Unit, normal_font);
                    tableContent.AddCell(cellRight);

                    cellRight.Phrase = new Phrase(detail.pricePerDealUnit.ToString("N", new CultureInfo("id-ID")), normal_font);
                    tableContent.AddCell(cellRight);

                    cellRight.Phrase = new Phrase(detail.priceTotal.ToString("N", new CultureInfo("id-ID")), normal_font);
                    tableContent.AddCell(cellRight);

                    totalPriceTotal += detail.priceTotal;
                    total = totalPriceTotal * detail.deliveryOrder.docurrency.Rate;

                    if (units.ContainsKey(detail.unit.Code))
                    {
                        units[detail.unit.Code] += detail.priceTotal;
                    }
                    else
                    {
                        units.Add(detail.unit.Code, detail.priceTotal);
                    }
                    
                    if (item.garmentInvoice.useVat)
                    {
                        ppn = 0.1 * totalPriceTotal;
                    }
                    
                    if (item.garmentInvoice.useIncomeTax)
                    {
                        pph = item.garmentInvoice.incomeTaxRate* totalPriceTotal;
                    }

                    maxtotal = pph + ppn + totalPriceTotal;
                }
            }
            
            PdfPCell cellContent = new PdfPCell(tableContent); // dont remove
            tableContent.ExtendLastRow = false;
            tableContent.SpacingAfter = 20f;
            document.Add(tableContent);
            #endregion

            #region Footer

            PdfPTable tableFooter = new PdfPTable(4);
            tableFooter.SetWidths(new float[] { 1.5f, 6f, 3f, 3f });

            PdfPCell cellInternNoteFooterLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell cellInternNoteFooterRight = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            

            foreach (var unit in units)
            {
                cellInternNoteFooterLeft.Phrase = new Phrase("Total "+unit.Key , normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterLeft.Phrase = new Phrase(" : " + unit.Value.ToString("N", new CultureInfo("id-ID")), normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);
            }

                cellInternNoteFooterRight.Phrase = new Phrase("Total Harga Pokok (DPP)", normal_font);
                tableFooter.AddCell(cellInternNoteFooterRight);

                cellInternNoteFooterLeft.Phrase = new Phrase(": " + totalPriceTotal.ToString("N", new CultureInfo("id-ID")), normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterLeft.Phrase = new Phrase("", normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterLeft.Phrase = new Phrase("", normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterRight.Phrase = new Phrase("Mata Uang" , normal_font);
                tableFooter.AddCell(cellInternNoteFooterRight);

                cellInternNoteFooterLeft.Phrase = new Phrase(": " + viewModel.currency.Code, normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterLeft.Phrase = new Phrase("", normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterLeft.Phrase = new Phrase("", normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterRight.Phrase = new Phrase("Total Harga Pokok (Rp)" , normal_font);
                tableFooter.AddCell(cellInternNoteFooterRight);

                cellInternNoteFooterLeft.Phrase = new Phrase(": " + total.ToString("N", new CultureInfo("id-ID")), normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterLeft.Phrase = new Phrase("", normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterLeft.Phrase = new Phrase("", normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterRight.Phrase = new Phrase("Total Nota Koreksi", normal_font);
                tableFooter.AddCell(cellInternNoteFooterRight);

                cellInternNoteFooterLeft.Phrase = new Phrase(": " + 0, normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterLeft.Phrase = new Phrase("", normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterLeft.Phrase = new Phrase("", normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterRight.Phrase = new Phrase("Total Nota PPn" , normal_font);
                tableFooter.AddCell(cellInternNoteFooterRight);

                cellInternNoteFooterLeft.Phrase = new Phrase(": " + ppn.ToString("N", new CultureInfo("id-ID")), normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterLeft.Phrase = new Phrase("", normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterLeft.Phrase = new Phrase("", normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterRight.Phrase = new Phrase("Total Nota PPh", normal_font);
                tableFooter.AddCell(cellInternNoteFooterRight);

                cellInternNoteFooterLeft.Phrase = new Phrase(": " + pph.ToString("N", new CultureInfo("id-ID")), normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterLeft.Phrase = new Phrase("", normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

                cellInternNoteFooterLeft.Phrase = new Phrase("", normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);

            cellInternNoteFooterRight.Phrase = new Phrase("Total yang Harus Dibayar" , normal_font);
                tableFooter.AddCell(cellInternNoteFooterRight);

                cellInternNoteFooterLeft.Phrase = new Phrase(": " + maxtotal.ToString("N", new CultureInfo("id-ID")), normal_font);
                tableFooter.AddCell(cellInternNoteFooterLeft);



            PdfPCell cellFooter = new PdfPCell(tableFooter); // dont remove
            tableFooter.ExtendLastRow = false;
            tableFooter.SpacingAfter = 20f;
            document.Add(tableFooter);

            #endregion

            #region TableSignature

            PdfPTable tableSignature = new PdfPTable(3);

            PdfPCell cellSignatureContent = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_CENTER };

            cellSignatureContent.Phrase = new Phrase("Administrasi\n\n\n\n\n\n\n(  " + "Nama & Tanggal" + "  )", bold_font);
            tableSignature.AddCell(cellSignatureContent);
            cellSignatureContent.Phrase = new Phrase("Staff Pembelian\n\n\n\n\n\n\n(  " + "Nama & Tanggal" + "  )", bold_font);
            tableSignature.AddCell(cellSignatureContent);
            cellSignatureContent.Phrase = new Phrase("Verifikasi\n\n\n\n\n\n\n(  " + "Nama & Tanggal" + "  )", bold_font);
            tableSignature.AddCell(cellSignatureContent);


            PdfPCell cellSignature = new PdfPCell(tableSignature); // dont remove
            tableSignature.ExtendLastRow = false;
            tableSignature.SpacingBefore = 20f;
            tableSignature.SpacingAfter = 20f;
            document.Add(tableSignature);

            #endregion

            document.Close();
            byte[] byteInfo = stream.ToArray();
            stream.Write(byteInfo, 0, byteInfo.Length);
            stream.Position = 0;

            return stream;
        }
    }
}
