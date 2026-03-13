
using System;

namespace Spendly.Shared.Core.Extensions;

public static class DateTimeExtensions
{
	public static DateOnly ToDateOnly(this DateTime dateTime) => DateOnly.FromDateTime(dateTime);

	public static DateOnly? ToDateOnly(this DateTime? dateTime) => dateTime.HasValue ? DateOnly.FromDateTime(dateTime.Value) : null;

	public static DateTime ToDateTime(this DateOnly dateOnly) => dateOnly.ToDateTime(TimeOnly.MinValue);

	public static DateTime? ToDateTime(this DateOnly? dateOnly) => dateOnly.HasValue ? dateOnly.Value.ToDateTime(TimeOnly.MinValue) : null;
}
