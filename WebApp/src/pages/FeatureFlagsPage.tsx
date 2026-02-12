import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { getApi, postApi, putApi, deleteApi } from "@/lib/api";
import { useI18n } from "@/i18n/provider";
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
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";
import { Trash2, Plus, Edit, Flag } from "lucide-react";

type FeatureFlag = {
  id: string;
  key: string;
  enabled: boolean;
  description?: string;
  createdAt: string;
  updatedAt?: string;
};

type CreateFlagDto = { key: string; enabled?: boolean; description?: string };
type UpdateFlagDto = { key?: string; enabled?: boolean; description?: string };

export function FeatureFlagsPage() {
  const { t } = useI18n();
  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [selected, setSelected] = useState<FeatureFlag | null>(null);

  const qc = useQueryClient();

  const q = useQuery({
    queryKey: ["admin", "feature-flags"],
    queryFn: async () => {
      const r = await getApi<FeatureFlag[]>("/admin/feature-flags");
      return r.data as FeatureFlag[];
    },
  });

  const createMut = useMutation({
    mutationFn: async (dto: CreateFlagDto) => {
      await postApi("/admin/feature-flags", dto);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin", "feature-flags"] });
      toast.success("Feature flag created");
      setCreateOpen(false);
    },
    onError: () => toast.error("Failed to create flag"),
  });

  const updateMut = useMutation({
    mutationFn: async ({ id, dto }: { id: string; dto: UpdateFlagDto }) => {
      await putApi(`/admin/feature-flags/${id}`, dto);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin", "feature-flags"] });
      toast.success("Feature flag updated");
      setEditOpen(false);
      setSelected(null);
    },
    onError: () => toast.error("Failed to update flag"),
  });

  const deleteMut = useMutation({
    mutationFn: async (id: string) => {
      await deleteApi(`/admin/feature-flags/${id}`);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin", "feature-flags"] });
      toast.success("Feature flag deleted");
    },
    onError: () => toast.error("Failed to delete flag"),
  });

  const handleEdit = (flag: FeatureFlag) => {
    setSelected(flag);
    setEditOpen(true);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this feature flag?")) {
      deleteMut.mutate(id);
    }
  };

  return (
    <PermissionGuard permission="flags.read">
      <Page
        title="Feature Flags"
        description="Manage feature flags"
        actions={
          <PermissionGuard permission="flags.write">
            <Button size="sm" onClick={() => setCreateOpen(true)}>
              <Plus className="w-4 h-4 mr-1" />
              Create Flag
            </Button>
          </PermissionGuard>
        }
      >
        {q.isLoading && <div className="text-sm text-muted-foreground">Loading…</div>}
        {q.isError && <div className="text-sm text-destructive">Error loading flags</div>}

        {!q.isLoading && !q.isError && (
          <div className="rounded-xl border overflow-hidden">
            <div className="divide-y">
              {q.data?.map((flag) => (
                <div key={flag.id} className="p-4 flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <Flag className="w-5 h-5 text-muted-foreground" />
                    <div>
                      <div className="font-medium">{flag.key}</div>
                      {flag.description && (
                        <div className="text-sm text-muted-foreground">{flag.description}</div>
                      )}
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Badge variant={flag.enabled ? "default" : "secondary"}>
                      {flag.enabled ? "Enabled" : "Disabled"}
                    </Badge>
                    <PermissionGuard permission="flags.write">
                      <Button variant="outline" size="sm" onClick={() => handleEdit(flag)}>
                        <Edit className="w-4 h-4" />
                      </Button>
                      <Button variant="outline" size="sm" onClick={() => handleDelete(flag.id)}>
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    </PermissionGuard>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Create Dialog */}
        <Dialog open={createOpen} onOpenChange={setCreateOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Create Feature Flag</DialogTitle>
            </DialogHeader>
            <form
              onSubmit={(e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                createMut.mutate({
                  key: fd.get("key") as string,
                  enabled: (fd.get("enabled") as string) === "on",
                  description: (fd.get("description") as string) || undefined,
                });
              }}
            >
              <Label htmlFor="key">Key</Label>
              <Input id="key" name="key" required autoFocus />
              <div className="mt-2 flex items-center gap-2">
                <Checkbox id="enabled" name="enabled" defaultChecked />
                <Label htmlFor="enabled">Enabled</Label>
              </div>
              <Label htmlFor="description" className="mt-2 block">Description (optional)</Label>
              <Input id="description" name="description" />
              <DialogFooter className="mt-4">
                <Button type="button" variant="outline" onClick={() => setCreateOpen(false)}>
                  Cancel
                </Button>
                <Button type="submit" disabled={createMut.isPending}>
                  Create
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>

        {/* Edit Dialog */}
        <Dialog open={editOpen} onOpenChange={setEditOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Edit Feature Flag</DialogTitle>
            </DialogHeader>
            {selected && (
              <form
                onSubmit={(e) => {
                  e.preventDefault();
                  const fd = new FormData(e.currentTarget);
                  updateMut.mutate({
                    id: selected.id,
                    dto: {
                      key: fd.get("key") as string,
                      enabled: (fd.get("enabled") as string) === "on",
                      description: (fd.get("description") as string) || undefined,
                    },
                  });
                }}
              >
                <Label htmlFor="edit-key">Key</Label>
                <Input id="edit-key" name="key" defaultValue={selected.key} required autoFocus />
                <div className="mt-2 flex items-center gap-2">
                  <Checkbox id="edit-enabled" name="enabled" defaultChecked={selected.enabled} />
                  <Label htmlFor="edit-enabled">Enabled</Label>
                </div>
                <Label htmlFor="edit-description" className="mt-2 block">Description (optional)</Label>
                <Input id="edit-description" name="description" defaultValue={selected.description ?? ""} />
                <DialogFooter className="mt-4">
                  <Button type="button" variant="outline" onClick={() => setEditOpen(false)}>
                    Cancel
                  </Button>
                  <Button type="submit" disabled={updateMut.isPending}>
                    Save
                  </Button>
                </DialogFooter>
              </form>
            )}
          </DialogContent>
        </Dialog>
      </Page>
    </PermissionGuard>
  );
}
