import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { getApi, postApi, putApi, deleteApi } from "@/lib/api";
import { Page } from "@/shared/components/Page";
import { PermissionGuard } from "@/shared/auth/PermissionGuard";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";
import { Settings, Trash2, Eye } from "lucide-react";

type Quota = {
  id: string;
  tenantId: string;
  tenantName: string;
  endpointKey: string;
  permitLimit: number;
  windowSeconds: number;
  burst: number;
  userId?: string;
  userEmail?: string;
  createdAt: string;
  updatedAt: string;
};

type Usage = {
  endpoint: string;
  remaining: number;
};

export default function RateLimitPage() {
  const queryClient = useQueryClient();
  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [usageOpen, setUsageOpen] = useState(false);
  const [selectedQuota, setSelectedQuota] = useState<Quota | null>(null);
  const [selectedTenantId, setSelectedTenantId] = useState<string | null>(null);

  const quotasQ = useQuery({
    queryKey: ["admin", "rate-limit", "quotas"],
    queryFn: () => getApi<Quota[]>("/api/admin/rate-limit/quotas"),
  });

  const createMut = useMutation({
    mutationFn: (data: {
      tenantId: string;
      endpointKey: string;
      permitLimit: number;
      windowSeconds: number;
      burst: number;
      userId?: string;
    }) => postApi("/api/admin/rate-limit/quotas", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "rate-limit", "quotas"] });
      setCreateOpen(false);
      toast.success("Quota created");
    },
    onError: () => toast.error("Failed to create quota"),
  });

  const updateMut = useMutation({
    mutationFn: (data: {
      id: string;
      permitLimit: number;
      windowSeconds: number;
      burst: number;
    }) => putApi(`/api/admin/rate-limit/quotas/${data.id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "rate-limit", "quotas"] });
      setEditOpen(false);
      toast.success("Quota updated");
    },
    onError: () => toast.error("Failed to update quota"),
  });

  const deleteMut = useMutation({
    mutationFn: (id: string) => deleteApi(`/api/admin/rate-limit/quotas/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin", "rate-limit", "quotas"] });
      toast.success("Quota deleted");
    },
    onError: () => toast.error("Failed to delete quota"),
  });

  return (
    <PermissionGuard permission="rate_limit.write">
      <Page title="Rate Limit Quotas" description="Manage per-tenant and per-user API quotas">
        <div className="flex justify-between items-center mb-6">
          <div>
            <Button onClick={() => setCreateOpen(true)}>Add Quota</Button>
          </div>
        </div>

        {quotasQ.isLoading && <div>Loading...</div>}
        {quotasQ.isError && <div>Error loading quotas</div>}

        {quotasQ.data && (
          <div className="space-y-4">
            {quotasQ.data.data.map((quota: Quota) => (
              <div key={quota.id} className="border rounded-lg p-4 flex justify-between items-center">
                <div>
                  <div className="font-medium">{quota.tenantName}</div>
                  {quota.userEmail && <div className="text-sm text-muted-foreground">User: {quota.userEmail}</div>}
                  <div className="flex gap-2 mt-1">
                    <Badge variant="outline">{quota.endpointKey}</Badge>
                    <Badge variant="secondary">{quota.permitLimit} / {quota.windowSeconds}s</Badge>
                    <Badge variant="outline">Burst: {quota.burst}</Badge>
                  </div>
                </div>
                <div className="flex gap-2">
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => {
                      setSelectedTenantId(quota.tenantId);
                      setUsageOpen(true);
                    }}
                  >
                    <Eye className="w-4 h-4" />
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => {
                      setSelectedQuota(quota);
                      setEditOpen(true);
                    }}
                  >
                    <Settings className="w-4 h-4" />
                  </Button>
                  <Button
                    size="sm"
                    variant="destructive"
                    onClick={() => deleteMut.mutate(quota.id)}
                    disabled={deleteMut.isPending}
                  >
                    <Trash2 className="w-4 h-4" />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Create Dialog */}
        <CreateQuotaDialog
          open={createOpen}
          onOpenChange={setCreateOpen}
          onSubmit={(data) => createMut.mutate(data)}
          isPending={createMut.isPending}
        />

        {/* Edit Dialog */}
        {selectedQuota && (
          <EditQuotaDialog
            open={editOpen}
            onOpenChange={setEditOpen}
            quota={selectedQuota}
            onSubmit={(data) =>
              updateMut.mutate({
                id: selectedQuota.id,
                ...data,
              })
            }
            isPending={updateMut.isPending}
          />
        )}

        {/* Usage Dialog */}
        {selectedTenantId && (
          <UsageDialog
            open={usageOpen}
            onOpenChange={setUsageOpen}
            tenantId={selectedTenantId}
          />
        )}
      </Page>
    </PermissionGuard>
  );
}

function CreateQuotaDialog({
  open,
  onOpenChange,
  onSubmit,
  isPending,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (data: any) => void;
  isPending: boolean;
}) {
  const [form, setForm] = useState({
    tenantId: "",
    endpointKey: "api",
    permitLimit: 100,
    windowSeconds: 60,
    burst: 20,
    userId: "",
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit({
      ...form,
      tenantId: form.tenantId,
      userId: form.userId || undefined,
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create Quota</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <Label>Tenant ID</Label>
            <Input
              value={form.tenantId}
              onChange={(e) => setForm({ ...form, tenantId: e.target.value })}
              required
            />
          </div>
          <div>
            <Label>Endpoint</Label>
            <select
              value={form.endpointKey}
              onChange={(e) => setForm({ ...form, endpointKey: e.target.value })}
              className="w-full p-2 border rounded"
            >
              <option value="auth">Auth</option>
              <option value="admin">Admin</option>
              <option value="api">API</option>
            </select>
          </div>
          <div className="grid grid-cols-3 gap-2">
            <div>
              <Label>Limit</Label>
              <Input
                type="number"
                value={form.permitLimit}
                onChange={(e) => setForm({ ...form, permitLimit: Number(e.target.value) })}
                required
              />
            </div>
            <div>
              <Label>Window (s)</Label>
              <Input
                type="number"
                value={form.windowSeconds}
                onChange={(e) => setForm({ ...form, windowSeconds: Number(e.target.value) })}
                required
              />
            </div>
            <div>
              <Label>Burst</Label>
              <Input
                type="number"
                value={form.burst}
                onChange={(e) => setForm({ ...form, burst: Number(e.target.value) })}
                required
              />
            </div>
          </div>
          <div>
            <Label>User ID (optional)</Label>
            <Input
              value={form.userId}
              onChange={(e) => setForm({ ...form, userId: e.target.value })}
              placeholder="Leave empty for tenant-wide"
            />
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={isPending}>
              Create
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function EditQuotaDialog({
  open,
  onOpenChange,
  quota,
  onSubmit,
  isPending,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  quota: Quota;
  onSubmit: (data: any) => void;
  isPending: boolean;
}) {
  const [form, setForm] = useState({
    permitLimit: quota.permitLimit,
    windowSeconds: quota.windowSeconds,
    burst: quota.burst,
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(form);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit Quota</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-3 gap-2">
            <div>
              <Label>Limit</Label>
              <Input
                type="number"
                value={form.permitLimit}
                onChange={(e) => setForm({ ...form, permitLimit: Number(e.target.value) })}
                required
              />
            </div>
            <div>
              <Label>Window (s)</Label>
              <Input
                type="number"
                value={form.windowSeconds}
                onChange={(e) => setForm({ ...form, windowSeconds: Number(e.target.value) })}
                required
              />
            </div>
            <div>
              <Label>Burst</Label>
              <Input
                type="number"
                value={form.burst}
                onChange={(e) => setForm({ ...form, burst: Number(e.target.value) })}
                required
              />
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={isPending}>
              Save
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function UsageDialog({
  open,
  onOpenChange,
  tenantId,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  tenantId: string;
}) {
  const usageQ = useQuery({
    queryKey: ["admin", "rate-limit", "tenants", tenantId, "usage"],
    queryFn: () => getApi<Usage[]>(`/api/admin/rate-limit/tenants/${tenantId}/usage`),
    enabled: open,
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Usage Overview</DialogTitle>
        </DialogHeader>
        {usageQ.isLoading && <div>Loading...</div>}
        {usageQ.data && (
          <div className="space-y-2">
            {usageQ.data.data.map((u: Usage) => (
              <div key={u.endpoint} className="flex justify-between">
                <span className="capitalize">{u.endpoint}</span>
                <span>{u.remaining} remaining</span>
              </div>
            ))}
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
