import {
  AdminUsageMetricsResponse,
  BatchIntakeReliabilityReport,
} from "src/app/services/common-api.service";

export interface CombinedUsageRow {
  system: string;
  tenant: string;
  rawCertificateCn: string;
  callCount: number;
  failedCount: number;
  successCount: number;
  failureRate: number;
  sourceIpCount: number;
  firstSeen?: string | null;
  lastSeen?: string | null;
  apiCallCount: number;
  batchRecordCount: number;
  batchRejectedCount: number;
}

export function buildCombinedMetrics(
  apiMetrics: AdminUsageMetricsResponse,
  batchMetrics: BatchIntakeReliabilityReport,
): CombinedUsageRow[] {
  const batchBySource = new Map<string, {
    tenant: string;
    batchRecordCount: number;
    batchRejectedCount: number;
    firstSeen?: string | null;
    lastSeen?: string | null;
  }>();

  for (const file of batchMetrics.files) {
    const sourceSystemName = (file.sourceSystemName || "Unknown").trim() || "Unknown";
    const key = `${sourceSystemName}|${(file.tenant || "").trim().toLowerCase()}`;
    const current = batchBySource.get(key) ?? {
      tenant: file.tenant || "",
      batchRecordCount: 0,
      batchRejectedCount: 0,
      firstSeen: null,
      lastSeen: null,
    };

    current.batchRecordCount += file.recordsCount || 0;
    current.batchRejectedCount += file.rejectCount || 0;

    const requestTime = file.requestDateTime || null;
    if (requestTime) {
      if (!current.firstSeen || new Date(requestTime).getTime() < new Date(current.firstSeen).getTime()) {
        current.firstSeen = requestTime;
      }

      if (!current.lastSeen || new Date(requestTime).getTime() > new Date(current.lastSeen).getTime()) {
        current.lastSeen = requestTime;
      }
    }

    batchBySource.set(key, current);
  }

  const combined = new Map<string, CombinedUsageRow>();

  for (const system of apiMetrics.systems) {
    const sourceSystemName = system.system || "Unknown";
    const key = `${sourceSystemName}|${(system.tenant || "").trim().toLowerCase()}`;
    const batch = batchBySource.get(key);
    const batchRecordCount = batch?.batchRecordCount ?? 0;
    const batchRejectedCount = batch?.batchRejectedCount ?? 0;
    const callCount = system.callCount + batchRecordCount;
    const failedCount = system.failedCount + batchRejectedCount;
    const successCount = system.successCount + Math.max(batchRecordCount - batchRejectedCount, 0);

    combined.set(key, {
      system: sourceSystemName,
      tenant: system.tenant,
      callCount,
      failedCount,
      successCount,
      failureRate: callCount > 0 ? Math.floor((failedCount * 10000) / callCount) / 100 : 0,
      sourceIpCount: system.sourceIpCount,
      firstSeen: earliestTimestamp(system.firstSeen, batch?.firstSeen ?? null),
      lastSeen: latestTimestamp(system.lastSeen, batch?.lastSeen ?? null),
      rawCertificateCn: system.rawCertificateCn,
      apiCallCount: system.callCount,
      batchRecordCount,
      batchRejectedCount,
    });
  }

  for (const [key, batch] of batchBySource.entries()) {
    if (combined.has(key)) {
      continue;
    }

    const sourceSystemName = key.split("|")[0];
    const callCount = batch.batchRecordCount;
    const failedCount = batch.batchRejectedCount;
    const successCount = Math.max(callCount - failedCount, 0);

    combined.set(key, {
      system: sourceSystemName,
      tenant: batch.tenant,
      callCount,
      failedCount,
      successCount,
      failureRate: callCount > 0 ? Math.floor((failedCount * 10000) / callCount) / 100 : 0,
      sourceIpCount: 0,
      firstSeen: batch.firstSeen ?? null,
      lastSeen: batch.lastSeen ?? null,
      rawCertificateCn: "",
      apiCallCount: 0,
      batchRecordCount: batch.batchRecordCount,
      batchRejectedCount: batch.batchRejectedCount,
    });
  }

  return Array.from(combined.values())
    .sort((left, right) => right.callCount - left.callCount || left.system.localeCompare(right.system));
}

function earliestTimestamp(left?: string | null, right?: string | null): string | null {
  if (!left) {
    return right ?? null;
  }

  if (!right) {
    return left;
  }

  return new Date(left).getTime() <= new Date(right).getTime() ? left : right;
}

function latestTimestamp(left?: string | null, right?: string | null): string | null {
  if (!left) {
    return right ?? null;
  }

  if (!right) {
    return left;
  }

  return new Date(left).getTime() >= new Date(right).getTime() ? left : right;
}
