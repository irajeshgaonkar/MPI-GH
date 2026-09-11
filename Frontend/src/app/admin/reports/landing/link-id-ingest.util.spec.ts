import { LinkIdIngestTrendRow } from "src/app/services/common-api.service";
import {
  buildLinkIdIngestChartData,
  buildLinkIdIngestSourceRows,
  buildLinkIdIngestView,
  filterLinkIdIngestViewByLookback,
  paginateItems,
  syncSelectedSources,
} from "./link-id-ingest.util";

const rows: LinkIdIngestTrendRow[] = [
  {
    date: "2026-08-01",
    sourceSystemName: "SourceA",
    incomingRecords: 100,
    newPersonRecords: 40,
    alreadyInMpiRecords: 60,
  },
  {
    date: "2026-08-01",
    sourceSystemName: "SourceB",
    incomingRecords: 50,
    newPersonRecords: 10,
    alreadyInMpiRecords: 40,
  },
  {
    date: "2026-08-02",
    sourceSystemName: "SourceA",
    incomingRecords: 20,
    newPersonRecords: 5,
    alreadyInMpiRecords: 15,
  },
];

describe("buildLinkIdIngestView", () => {
  it("rolls up all selected sources by day without averaging percents", () => {
    const view = buildLinkIdIngestView(rows, ["SourceA", "SourceB"]);

    expect(view.sourceOptions).toEqual(["SourceA", "SourceB"]);
    expect(view.dailyRows.length).toBe(2);
    expect(view.dailyRows[0]).toEqual({
      date: "2026-08-01",
      incomingRecords: 150,
      newPersonRecords: 50,
      alreadyInMpiRecords: 100,
      newPersonPercent: 33,
      alreadyInMpiPercent: 67,
    });
    expect(view.totals.incomingRecords).toBe(170);
    expect(view.totals.newPersonRecords).toBe(55);
    expect(view.totals.alreadyInMpiRecords).toBe(115);
    expect(view.totals.newPersonPercent + view.totals.alreadyInMpiPercent).toBe(100);
  });

  it("filters to one source while keeping counts intact", () => {
    const view = buildLinkIdIngestView(rows, ["SourceB"]);

    expect(view.dailyRows.length).toBe(1);
    expect(view.dailyRows[0].incomingRecords).toBe(50);
    expect(view.dailyRows[0].newPersonRecords).toBe(10);
    expect(view.dailyRows[0].alreadyInMpiRecords).toBe(40);
    expect(view.dailyRows[0].newPersonPercent).toBe(20);
    expect(view.dailyRows[0].alreadyInMpiPercent).toBe(80);
  });

  it("rolls up a multi-source subset", () => {
    const view = buildLinkIdIngestView(rows, ["SourceA"]);

    expect(view.dailyRows.length).toBe(2);
    expect(view.totals.incomingRecords).toBe(120);
  });

  it("returns empty daily rows when nothing is selected", () => {
    const view = buildLinkIdIngestView(rows, []);

    expect(view.sourceOptions).toEqual(["SourceA", "SourceB"]);
    expect(view.dailyRows).toEqual([]);
    expect(view.totals.incomingRecords).toBe(0);
  });

  it("keeps new plus already equal to incoming for each row", () => {
    const view = buildLinkIdIngestView(rows, ["SourceA", "SourceB"]);

    for (const row of [...view.dailyRows, view.totals]) {
      expect(row.newPersonRecords + row.alreadyInMpiRecords).toBe(row.incomingRecords);
    }
  });
});

describe("buildLinkIdIngestSourceRows", () => {
  it("rolls up by source within the selection", () => {
    const sourceRows = buildLinkIdIngestSourceRows(rows, ["SourceA", "SourceB"]);

    expect(sourceRows.length).toBe(2);
    expect(sourceRows[0].sourceSystemName).toBe("SourceA");
    expect(sourceRows[0].incomingRecords).toBe(120);
    expect(sourceRows[1].sourceSystemName).toBe("SourceB");
    expect(sourceRows[1].incomingRecords).toBe(50);
  });

  it("returns empty when a single selected source list is empty", () => {
    expect(buildLinkIdIngestSourceRows(rows, [])).toEqual([]);
  });
});

describe("buildLinkIdIngestChartData", () => {
  it("returns empty array for no daily rows", () => {
    expect(buildLinkIdIngestChartData([])).toEqual([]);
  });

  it("maps daily rows to chart keys in chronological order", () => {
    const view = buildLinkIdIngestView(rows, ["SourceA", "SourceB"]);
    const chartData = buildLinkIdIngestChartData(view.dailyRows);

    expect(chartData.length).toBe(2);
    expect(chartData[0].label).toBeTruthy();
    expect(chartData[0].newPerson).toBe(50);
    expect(chartData[0].alreadyInMpi).toBe(100);
    expect(chartData[0].alreadyInMpiPercent).toBe(67);
    expect(chartData[0].incoming).toBe(150);
    expect(chartData[0].sourceBreakdown).toBeUndefined();
    expect(chartData[1].newPerson).toBe(5);
    expect(chartData[1].alreadyInMpi).toBe(15);
    expect(chartData[1].alreadyInMpiPercent).toBe(75);
    expect(chartData[1].incoming).toBe(20);
  });

  it("includes source breakdown when requested", () => {
    const view = buildLinkIdIngestView(rows, ["SourceA", "SourceB"]);
    const chartData = buildLinkIdIngestChartData(
      view.dailyRows,
      rows,
      ["SourceA", "SourceB"],
      true,
    );

    expect(chartData[0].sourceBreakdown?.length).toBe(2);
    expect(chartData[0].sourceBreakdown?.[0].source).toBe("SourceA");
    expect(chartData[0].sourceBreakdown?.[0].incoming).toBe(100);
    expect(chartData[0].sourceBreakdown?.[1].source).toBe("SourceB");
    expect(chartData[0].sourceBreakdown?.[1].incoming).toBe(50);
  });
});

describe("filterLinkIdIngestViewByLookback", () => {
  it("keeps only rows within the selected lookback window", () => {
    const view = buildLinkIdIngestView(rows, ["SourceA", "SourceB"]);
    const referenceDate = new Date("2026-08-03T12:00:00");
    const filtered = filterLinkIdIngestViewByLookback(view, 1, referenceDate);

    expect(filtered.dailyRows.length).toBe(1);
    expect(filtered.dailyRows[0].date).toBe("2026-08-02");
    expect(filtered.totals.incomingRecords).toBe(20);
  });
});

describe("paginateItems", () => {
  it("slices page items while preserving total count for sticky totals", () => {
    const items = Array.from({ length: 25 }, (_, index) => index + 1);
    const page = paginateItems(items, 1, 10);

    expect(page.pageItems).toEqual([11, 12, 13, 14, 15, 16, 17, 18, 19, 20]);
    expect(page.totalCount).toBe(25);
    expect(page.pageIndex).toBe(1);
    expect(page.pageCount).toBe(3);
  });

  it("clamps page index to valid range", () => {
    const page = paginateItems([1, 2, 3], 99, 10);

    expect(page.pageIndex).toBe(0);
    expect(page.pageItems).toEqual([1, 2, 3]);
  });
});

describe("syncSelectedSources", () => {
  it("defaults to all options on first load", () => {
    expect(syncSelectedSources([], ["SourceA", "SourceB"], false, true)).toEqual(["SourceA", "SourceB"]);
  });

  it("keeps all options when previously all selected", () => {
    expect(syncSelectedSources(["SourceA"], ["SourceA", "SourceB", "SourceC"], true, false))
      .toEqual(["SourceA", "SourceB", "SourceC"]);
  });

  it("keeps subset intersection when not all selected", () => {
    expect(syncSelectedSources(["SourceA", "Gone"], ["SourceA", "SourceB"], false, false))
      .toEqual(["SourceA"]);
  });

  it("preserves intentional empty selection on refresh", () => {
    expect(syncSelectedSources([], ["SourceA", "SourceB"], false, false)).toEqual([]);
  });
});
