import { useMemo, useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Truck, Undo2, XCircle } from "lucide-react";
import DataGrid, { DataGridColumn } from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import DateInput from "../../components/shared/DateInput";
import { useProjectLookup } from "../../hooks/useEntityLookup";
import {
  getAssetDeployments,
  getFixedAssets,
  deployAsset,
  returnAsset,
  cancelAssetDeployment,
  AssetDeployment,
  DeploymentChargeMode,
  HireRateUnit,
  FixedAssetStatus,
  chargeModeLabels,
  hireRateUnitLabels,
  fixedAssetStatusLabels,
} from "../../api/accounts";

const inputClass = "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500";
const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const today = () => new Date().toISOString().split("T")[0];
const day = (iso?: string) => iso?.split("T")[0] ?? "";

type ReturnStatus = Exclude<FixedAssetStatus, "Deployed">;

function errorMessage(err: unknown, fallback: string) {
  const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
  return axiosErr?.response?.data?.message ?? axiosErr?.response?.data?.errors?.[0] ?? fallback;
}

const emptyDeploy = {
  fixedAssetId: "",
  projectId: "",
  startDate: today(),
  chargeMode: "HireRate" as DeploymentChargeMode,
  rateUnit: "PerDay" as HireRateUnit,
  rate: 0,
  notes: "",
};

export default function AssetDeployments() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "startDate", defaultSortDescending: true });
  const [openOnly, setOpenOnly] = useState(true);
  const [deployOpen, setDeployOpen] = useState(false);
  const [deployForm, setDeployForm] = useState(emptyDeploy);
  const [returning, setReturning] = useState<AssetDeployment | null>(null);
  const [returnForm, setReturnForm] = useState({ endDate: today(), returnLocation: "", returnStatus: "Active" as ReturnStatus, returnNotes: "" });
  const [error, setError] = useState("");

  const { options: projectOptions, isLoading: projectsLoading } = useProjectLookup();

  const { data, isLoading } = useQuery({
    queryKey: ["asset-deployments", ...dg.queryKey, openOnly],
    queryFn: () => getAssetDeployments(dg.queryParams, { openOnly }),
    placeholderData: (prev) => prev,
  });
  const deployments = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  // Only assets that can actually go to site: not already out, not in the workshop.
  const { data: assetsData } = useQuery({
    queryKey: ["fixed-assets-deployable"],
    queryFn: () => getFixedAssets({ page: 1, pageSize: 100 }),
    enabled: deployOpen,
  });
  const assetOptions = useMemo(
    () => (assetsData?.data?.data?.items ?? [])
      .filter((a) => a.status === "Active" || a.status === "Idle")
      .map((a) => ({ value: a.id, label: `${a.assetCode} — ${a.name}`, searchText: `${a.assetCode} ${a.name} ${a.registrationNumber ?? ""} ${a.chassisNumber ?? ""}` })),
    [assetsData],
  );

  function invalidate() {
    qc.invalidateQueries({ queryKey: ["asset-deployments"] });
    qc.invalidateQueries({ queryKey: ["fixed-assets"] });
    qc.invalidateQueries({ queryKey: ["fixed-assets-deployable"] });
  }

  const deployMutation = useMutation({
    mutationFn: () => deployAsset({
      fixedAssetId: deployForm.fixedAssetId,
      projectId: deployForm.projectId,
      startDate: deployForm.startDate,
      chargeMode: deployForm.chargeMode,
      rateUnit: deployForm.chargeMode === "HireRate" ? deployForm.rateUnit : undefined,
      rate: deployForm.chargeMode === "HireRate" ? deployForm.rate : undefined,
      notes: deployForm.notes || undefined,
    }),
    onSuccess: () => { invalidate(); setDeployOpen(false); },
    onError: (err) => setError(errorMessage(err, "Failed to deploy asset.")),
  });

  const returnMutation = useMutation({
    mutationFn: () => returnAsset(returning!.id, {
      endDate: returnForm.endDate,
      returnLocation: returnForm.returnLocation || undefined,
      returnStatus: returnForm.returnStatus,
      returnNotes: returnForm.returnNotes || undefined,
    }),
    onSuccess: () => { invalidate(); setReturning(null); },
    onError: (err) => setError(errorMessage(err, "Failed to return asset.")),
  });

  const cancelMutation = useMutation({
    mutationFn: (id: string) => cancelAssetDeployment(id),
    onSuccess: invalidate,
    onError: (err) => window.alert(errorMessage(err, "Failed to cancel deployment.")),
  });

  function openDeploy() {
    setDeployForm({ ...emptyDeploy, startDate: today() });
    setError("");
    setDeployOpen(true);
  }

  function openReturn(d: AssetDeployment) {
    setReturning(d);
    setReturnForm({ endDate: today(), returnLocation: "", returnStatus: "Active", returnNotes: "" });
    setError("");
  }

  function confirmCancel(d: AssetDeployment) {
    if (window.confirm(`Cancel the deployment of ${d.fixedAssetName} to ${d.costCenterName}? Use this only for a deployment recorded by mistake — it is removed as if it never happened.`))
      cancelMutation.mutate(d.id);
  }

  const columns: DataGridColumn<AssetDeployment>[] = [
    {
      key: "fixedAssetName",
      label: "Asset",
      render: (row) => (
        <div>
          <div className="text-gray-900">{row.fixedAssetName}</div>
          <div className="text-xs font-mono text-gray-400">{row.fixedAssetAssetCode}</div>
        </div>
      ),
    },
    { key: "costCenterName", label: "Project" },
    { key: "startDate", label: "From", render: (row) => day(row.startDate) },
    {
      key: "endDate",
      label: "To",
      render: (row) => row.isOpen ? <span className="text-xs px-2 py-0.5 rounded-full bg-blue-50 text-blue-700">On site</span> : day(row.endDate),
    },
    {
      key: "chargeMode",
      label: "Charging",
      sortable: false,
      render: (row) => row.chargeMode === "HireRate" && row.rate != null && row.rateUnit
        ? <span className="tabular-nums">{fmt(row.rate)} {hireRateUnitLabels[row.rateUnit]}</span>
        : <span className="text-gray-500">{chargeModeLabels[row.chargeMode]}</span>,
    },
    {
      key: "chargedUpTo",
      label: "Hire Charged",
      sortable: false,
      className: "text-right tabular-nums",
      headerClassName: "text-right",
      render: (row) => row.chargeMode !== "HireRate" ? "—" : (
        <div>
          <div>{fmt(row.totalHireCharged)}</div>
          <div className="text-xs text-gray-400">{row.chargedUpTo ? `up to ${day(row.chargedUpTo)}` : "nothing yet"}</div>
        </div>
      ),
    },
    {
      key: "actions",
      label: "Actions",
      sortable: false,
      render: (row) => (
        <div className="flex items-center gap-2">
          {row.isOpen && (
            <button onClick={() => openReturn(row)} className="text-primary-600 hover:text-primary-800" title="Return from site">
              <Undo2 size={14} />
            </button>
          )}
          {!row.chargedUpTo && (
            <button onClick={() => confirmCancel(row)} className="text-red-400 hover:text-red-600" title="Cancel (recorded by mistake)">
              <XCircle size={14} />
            </button>
          )}
        </div>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Asset Deployments</h2>
          <p className="text-xs text-gray-400">Which machine is on which project, since when, and how that project pays for it</p>
        </div>
        <button onClick={openDeploy} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Truck size={15} /> Deploy to Project
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={deployments}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search asset or project..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        emptyMessage={openOnly ? "No machines are on a project right now" : "No deployments recorded"}
        filterBar={
          <label className="flex items-center gap-2 text-sm text-gray-600">
            <input type="checkbox" checked={openOnly} onChange={(e) => { setOpenOnly(e.target.checked); dg.resetPage(); }} />
            Currently on site only
          </label>
        }
      />

      <Modal title="Deploy Asset to Project" open={deployOpen} onClose={() => setDeployOpen(false)} size="2xl">
        <div className="space-y-3">
          {error && <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{error}</div>}
          <SearchableDropdown
            label="Asset *"
            options={assetOptions}
            value={deployForm.fixedAssetId}
            onChange={(v) => setDeployForm((f) => ({ ...f, fixedAssetId: v ?? "" }))}
            placeholder="Select an available asset"
            searchPlaceholder="Search code, name, registration..."
            required
          />
          <div className="grid grid-cols-2 gap-3">
            <SearchableDropdown
              label="Project *"
              options={projectOptions}
              value={deployForm.projectId}
              onChange={(v) => setDeployForm((f) => ({ ...f, projectId: v ?? "" }))}
              placeholder="Select project"
              searchPlaceholder="Search projects..."
              loading={projectsLoading}
              required
            />
            <div>
              <label className="text-xs text-gray-500 mb-1 block">On site from *</label>
              <DateInput className={inputClass} value={deployForm.startDate} onChange={(e) => setDeployForm((f) => ({ ...f, startDate: e.target.value }))} />
            </div>
          </div>

          <div>
            <label className="text-xs text-gray-500 mb-1 block">How does the project pay for this machine?</label>
            <div className="grid grid-cols-3 gap-2">
              {(Object.keys(chargeModeLabels) as DeploymentChargeMode[]).map((m) => (
                <button
                  key={m}
                  type="button"
                  onClick={() => setDeployForm((f) => ({ ...f, chargeMode: m }))}
                  className={`text-left border rounded-lg px-3 py-2 text-sm ${deployForm.chargeMode === m ? "border-primary-500 bg-primary-50 text-primary-700" : "border-gray-200 text-gray-600 hover:bg-gray-50"}`}
                >
                  <div className="font-medium">{chargeModeLabels[m]}</div>
                  <div className="text-[11px] text-gray-400 mt-0.5">
                    {m === "HireRate" && "Charged per day on site via Equipment Hire runs"}
                    {m === "RunningCostsOnly" && "Only fuel, operator and repairs, as project expenses"}
                    {m === "None" && "All cost stays at company level"}
                  </div>
                </button>
              ))}
            </div>
          </div>

          {deployForm.chargeMode === "HireRate" && (
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Hire Rate *</label>
                <input type="number" min={0} className={inputClass} value={deployForm.rate || ""} onChange={(e) => setDeployForm((f) => ({ ...f, rate: parseFloat(e.target.value) || 0 }))} />
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Per</label>
                <select className={inputClass} value={deployForm.rateUnit} onChange={(e) => setDeployForm((f) => ({ ...f, rateUnit: e.target.value as HireRateUnit }))}>
                  <option value="PerDay">Day</option>
                  <option value="PerMonth">Month (prorated by day)</option>
                </select>
              </div>
            </div>
          )}

          <div>
            <label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea rows={2} className={inputClass} value={deployForm.notes} onChange={(e) => setDeployForm((f) => ({ ...f, notes: e.target.value }))} />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={() => setDeployOpen(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => deployMutation.mutate()} disabled={deployMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {deployMutation.isPending ? "Deploying..." : "Deploy"}
            </button>
          </div>
        </div>
      </Modal>

      <Modal title={returning ? `Return ${returning.fixedAssetName} from ${returning.costCenterName}` : "Return"} open={returning !== null} onClose={() => setReturning(null)}>
        <div className="space-y-3">
          {error && <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{error}</div>}
          {returning?.chargedUpTo && (
            <p className="text-xs text-gray-500">Hire is already charged up to {day(returning.chargedUpTo)}, so the last day on site can't be earlier.</p>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Last day on site *</label>
              <DateInput className={inputClass} value={returnForm.endDate} onChange={(e) => setReturnForm((f) => ({ ...f, endDate: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Status after return</label>
              <select className={inputClass} value={returnForm.returnStatus} onChange={(e) => setReturnForm((f) => ({ ...f, returnStatus: e.target.value as ReturnStatus }))}>
                {(["Active", "Idle", "UnderMaintenance"] as ReturnStatus[]).map((s) => (
                  <option key={s} value={s}>{fixedAssetStatusLabels[s]}</option>
                ))}
              </select>
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Returned to</label>
            <input className={inputClass} placeholder="Blank = where it was before (e.g. CFEL Warehouse 1)" value={returnForm.returnLocation} onChange={(e) => setReturnForm((f) => ({ ...f, returnLocation: e.target.value }))} />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea rows={2} className={inputClass} value={returnForm.returnNotes} onChange={(e) => setReturnForm((f) => ({ ...f, returnNotes: e.target.value }))} />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={() => setReturning(null)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => returnMutation.mutate()} disabled={returnMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {returnMutation.isPending ? "Saving..." : "Record Return"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
