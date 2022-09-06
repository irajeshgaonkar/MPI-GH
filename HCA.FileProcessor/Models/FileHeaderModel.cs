using HCA.FileProcessor.Attributes;
using HCA.FileProcessor.Enums;
using HCA.Models.Enums;

namespace HCA.FileProcessor.Models;

/// <summary>
/// File Header Data Model
/// </summary>
[LineParser(ParserType.Header)]
public class FileHeaderModel
{
    /// <summary>
    /// <see cref="FileHeaderModel"/>
    /// </summary>
    public FileHeaderModel()
    {
    }

    /// <summary>
    /// Source System Agencys
    /// </summary>
    [FieldPosition(0)]
    [FieldValidator(ValidationType.Required, ErrorMessage = "Source system agency is Required")]
    public string SourceSystemAgency { get; set; }

    /// <summary>
    /// Source System Name
    /// </summary>
    [FieldPosition(1)]
    [FieldValidator(ValidationType.Required, ErrorMessage = "Source system name is required")]
    public string SourceSystemName { get; set; }

    /// <summary>
    /// File Created Date
    /// </summary>
    [FieldPosition(2)]
    [FieldValidator(ValidationType.Required, ErrorMessage = "File created date is required")]
    [FieldValidator(ValidationType.Date, ErrorMessage = "Should be a valid date")]
    public DateOnly FileCreatedDate { get; set; }

    /// <summary>
    /// File Created Time
    /// </summary>
    [FieldPosition(3)]
    [FieldValidator(ValidationType.Required, ErrorMessage = "File created time is required")]
    [FieldValidator(ValidationType.Date, ErrorMessage = "Should be a valid time")]
    public TimeOnly FileCreatedTime { get; set; }

    /// <summary>
    /// Operation type for the file <see cref="ApiCallType"/>
    /// </summary>
    [FieldPosition(4)]
    public ApiCallType ApiCallType { get; set; }

    /// <summary>
    /// Tracking Id for the file - It will be request Id in the table
    /// </summary>
    [FieldPosition(5)]
    public string TrackingId { get; set; }
}