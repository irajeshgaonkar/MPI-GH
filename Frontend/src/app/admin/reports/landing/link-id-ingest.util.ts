import { LinkIdIngestTrendRow } from "src/app/services/common-api.service";

export const LINK_ID_INGEST_PAGE_SIZE = 10;

export interface LinkIdIngestDailyRow {
  date: string;
  incomingRecords: number;
  newPersonRecords: number;
  alreadyInMpiRecords: number;
  newPersonPercent: number;
  alreadyInMpiPercent: number;
}

export interface LinkIdIngestSourceRow {
  sourceSystemName: string;
  incomingRecords: number;
  newPersonRecords: number;
  alreadyInMpiRecords: number;
  newPersonPercent: number;
  alreadyInMpiPercent: number;
}

export interface LinkIdIngestView {
  sourceOptions: string[];
  dailyRows: LinkIdIngestDailyRow[];
  totals: LinkIdIngestDailyRow;
}

export interface LinkIdIngestChartSourceBreakdown {
  source: string;
  incoming: number;
  newPerson: number;
  alreadyInMpi: number;
  alreadyInMpiPercent: number;
}

export interface LinkIdIngestChartPoint {
  label: string;
  newPerson: number;
  alreadyInMpi: number;
  alreadyInMpiPercent: number;
  incoming: number;
  sourceBreakdown?: LinkIdIngestChartSourceBreakdown[];
}

export interface PaginatedItems<T> {
  pageItems: T[];
  totalCount: number;
  pageIndex: number;
  pageCount: number;
}

export function extractLinkIdIngestSourceOptions(rows: LinkIdIngestTrendRow[]): string[] {
  return Array.from(
    new Set(rows.map((row) => row.sourceSystemName).filter((name) => !!name)),
  ).sort((left, right) => left.localeCompare(right));
}

export function buildLinkIdIngestView(
  rows: LinkIdIngestTrendRow[],
  selectedSources: string[],
): LinkIdIngestView {
  const sourceOptions = extractLinkIdIngestSourceOptions(rows);
  const filtered = filterRowsBySelectedSources(rows, selectedSources);
  const byDate = new Map<string, { incoming: number; newPerson: number; alreadyInMpi: number }>();

  for (const row of filtered) {
    const dateKey = normalizeDateKey(row.date);
    const current = byDate.get(dateKey) ?? { incoming: 0, newPerson: 0, alreadyInMpi: 0 };
    current.incoming += row.incomingRecords;
    current.newPerson += row.newPersonRecords;
    current.alreadyInMpi += row.alreadyInMpiRecords;
    byDate.set(dateKey, current);
  }

  const dailyRows = Array.from(byDate.entries())
    .map(([date, counts]) => toDailyRow(date, counts.incoming, counts.newPerson, counts.alreadyInMpi))
    .sort((left, right) => left.date.localeCompare(right.date));

  return {
    sourceOptions,
    dailyRows,
    totals: totalsFromCountRows(dailyRows),
  };
}

export function buildLinkIdIngestSourceRows(
  rows: LinkIdIngestTrendRow[],
  selectedSources: string[],
): LinkIdIngestSourceRow[] {
  const filtered = filterRowsBySelectedSources(rows, selectedSources);
  const bySource = new Map<string, { incoming: number; newPerson: number; alreadyInMpi: number }>();

  for (const row of filtered) {
    const name = row.sourceSystemName || "Unknown";
    const current = bySource.get(name) ?? { incoming: 0, newPerson: 0, alreadyInMpi: 0 };
    current.incoming += row.incomingRecords;
    current.newPerson += row.newPersonRecords;
    current.alreadyInMpi += row.alreadyInMpiRecords;
    bySource.set(name, current);
  }

  return Array.from(bySource.entries())
    .map(([sourceSystemName, counts]) => {
      const daily = toDailyRow("source", counts.incoming, counts.newPerson, counts.alreadyInMpi);
      return {
        sourceSystemName,
        incomingRecords: daily.incomingRecords,
        newPersonRecords: daily.newPersonRecords,
        alreadyInMpiRecords: daily.alreadyInMpiRecords,
        newPersonPercent: daily.newPersonPercent,
        alreadyInMpiPercent: daily.alreadyInMpiPercent,
      };
    })
    .sort((left, right) => {
      if (right.incomingRecords !== left.incomingRecords) {
        return right.incomingRecords - left.incomingRecords;
      }

      return left.sourceSystemName.localeCompare(right.sourceSystemName);
    });
}

export function buildLinkIdIngestChartData(
  dailyRows: LinkIdIngestDailyRow[],
  rawRows: LinkIdIngestTrendRow[] = [],
  selectedSources: string[] = [],
  includeSourceBreakdown = false,
): LinkIdIngestChartPoint[] {
  const breakdownByDate = includeSourceBreakdown
    ? buildSourceBreakdownByDate(rawRows, selectedSources)
    : null;

  return dailyRows.map((row) => {
    const point: LinkIdIngestChartPoint = {
      label: formatIngestChartLabel(row.date),
      newPerson: row.newPersonRecords,
      alreadyInMpi: row.alreadyInMpiRecords,
      alreadyInMpiPercent: row.alreadyInMpiPercent,
      incoming: row.incomingRecords,
    };

    if (breakdownByDate) {
      point.sourceBreakdown = breakdownByDate.get(row.date) ?? [];
    }

    return point;
  });
}

export function filterLinkIdIngestViewByLookback(
  view: LinkIdIngestView,
  lookbackDays: number,
  referenceDate: Date = new Date(),
): LinkIdIngestView {
  if (lookbackDays <= 0) {
    return {
      sourceOptions: view.sourceOptions,
      dailyRows: [],
      totals: toDailyRow("total", 0, 0, 0),
    };
  }

  const cutoffKey = getIngestLookbackCutoffDateKey(lookbackDays, referenceDate);
  const dailyRows = view.dailyRows.filter((row) => row.date >= cutoffKey);

  return {
    sourceOptions: view.sourceOptions,
    dailyRows,
    totals: totalsFromCountRows(dailyRows),
  };
}

export function filterTrendRowsByLookback(
  rows: LinkIdIngestTrendRow[],
  lookbackDays: number,
  referenceDate: Date = new Date(),
): LinkIdIngestTrendRow[] {
  if (lookbackDays <= 0) {
    return [];
  }

  const cutoffKey = getIngestLookbackCutoffDateKey(lookbackDays, referenceDate);
  return rows.filter((row) => normalizeDateKey(row.date) >= cutoffKey);
}

export function paginateItems<T>(
  items: T[],
  pageIndex: number,
  pageSize: number,
): PaginatedItems<T> {
  const totalCount = items.length;
  const safePageSize = pageSize > 0 ? pageSize : LINK_ID_INGEST_PAGE_SIZE;
  const pageCount = totalCount === 0 ? 0 : Math.ceil(totalCount / safePageSize);
  const safePageIndex = pageCount === 0
    ? 0
    : Math.min(Math.max(pageIndex, 0), pageCount - 1);
  const start = safePageIndex * safePageSize;

  return {
    pageItems: items.slice(start, start + safePageSize),
    totalCount,
    pageIndex: safePageIndex,
    pageCount,
  };
}

export function syncSelectedSources(
  previousSelected: string[],
  sourceOptions: string[],
  previouslyAllSelected: boolean,
  isInitial = false,
): string[] {
  if (sourceOptions.length === 0) {
    return [];
  }

  if (isInitial || previouslyAllSelected) {
    return [...sourceOptions];
  }

  const optionSet = new Set(sourceOptions);
  return previousSelected.filter((name) => optionSet.has(name));
}

export function formatIngestCountWithPercent(count: number, percent: number): string {
  return `${count} (${percent}%)`;
}

function filterRowsBySelectedSources(
  rows: LinkIdIngestTrendRow[],
  selectedSources: string[],
): LinkIdIngestTrendRow[] {
  if (selectedSources.length === 0) {
    return [];
  }

  const selected = new Set(selectedSources);
  return rows.filter((row) => selected.has(row.sourceSystemName));
}

function buildSourceBreakdownByDate(
  rows: LinkIdIngestTrendRow[],
  selectedSources: string[],
): Map<string, LinkIdIngestChartSourceBreakdown[]> {
  const filtered = filterRowsBySelectedSources(rows, selectedSources);
  const byDate = new Map<string, Map<string, { incoming: number; newPerson: number; alreadyInMpi: number }>>();

  for (const row of filtered) {
    const dateKey = normalizeDateKey(row.date);
    const sourceMap = byDate.get(dateKey) ?? new Map();
    const name = row.sourceSystemName || "Unknown";
    const current = sourceMap.get(name) ?? { incoming: 0, newPerson: 0, alreadyInMpi: 0 };
    current.incoming += row.incomingRecords;
    current.newPerson += row.newPersonRecords;
    current.alreadyInMpi += row.alreadyInMpiRecords;
    sourceMap.set(name, current);
    byDate.set(dateKey, sourceMap);
  }

  const result = new Map<string, LinkIdIngestChartSourceBreakdown[]>();

  for (const [date, sourceMap] of byDate.entries()) {
    const breakdown = Array.from(sourceMap.entries())
      .map(([source, counts]) => {
        const daily = toDailyRow(date, counts.incoming, counts.newPerson, counts.alreadyInMpi);
        return {
          source,
          incoming: daily.incomingRecords,
          newPerson: daily.newPersonRecords,
          alreadyInMpi: daily.alreadyInMpiRecords,
          alreadyInMpiPercent: daily.alreadyInMpiPercent,
        };
      })
      .sort((left, right) => {
        if (right.incoming !== left.incoming) {
          return right.incoming - left.incoming;
        }

        return left.source.localeCompare(right.source);
      });

    result.set(date, breakdown);
  }

  return result;
}

function totalsFromCountRows(
  rows: Array<{ incomingRecords: number; newPersonRecords: number; alreadyInMpiRecords: number }>,
): LinkIdIngestDailyRow {
  const totalIncoming = rows.reduce((sum, row) => sum + row.incomingRecords, 0);
  const totalNewPerson = rows.reduce((sum, row) => sum + row.newPersonRecords, 0);
  const totalAlreadyInMpi = rows.reduce((sum, row) => sum + row.alreadyInMpiRecords, 0);
  return toDailyRow("total", totalIncoming, totalNewPerson, totalAlreadyInMpi);
}

function formatIngestChartLabel(dateKey: string): string {
  if (!dateKey || dateKey === "total") {
    return "";
  }

  return new Date(`${dateKey}T00:00:00`).toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
  });
}

function getIngestLookbackCutoffDateKey(lookbackDays: number, referenceDate: Date): string {
  const cutoff = new Date(referenceDate);
  cutoff.setHours(0, 0, 0, 0);
  cutoff.setDate(cutoff.getDate() - lookbackDays);
  return toDateKey(cutoff);
}

function toDateKey(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

function normalizeDateKey(value: string): string {
  if (!value) {
    return "";
  }

  return value.slice(0, 10);
}

function toDailyRow(
  date: string,
  incomingRecords: number,
  newPersonRecords: number,
  alreadyInMpiRecords: number,
): LinkIdIngestDailyRow {
  const newPersonPercent = incomingRecords > 0
    ? Math.floor((newPersonRecords * 100) / incomingRecords)
    : 0;
  const alreadyInMpiPercent = incomingRecords > 0
    ? 100 - newPersonPercent
    : 0;

  return {
    date,
    incomingRecords,
    newPersonRecords,
    alreadyInMpiRecords,
    newPersonPercent,
    alreadyInMpiPercent,
  };
}
