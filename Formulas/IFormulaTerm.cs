namespace TextExcel3.Formulas;

/// <summary>
/// Represents the fundamental unit of a formula. A formula term can be a cell reference, a value (primitive data type-- integer, decimal, etc.), or a formula function. All formula terms must be able to be represented as a .NET `decimal` type.
/// </summary>
public interface IFormulaTerm
{
    public decimal DecimalValue { get; }
}