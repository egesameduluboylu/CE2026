import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { postApi } from "@/lib/api";
import { useI18n } from "@/i18n/provider";
import { Page } from "@/shared/components/Page";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";

type CreateUserReq = {
  email: string;
  password: string;
  isAdmin: boolean;
};

export function NewUserPage() {
  const { t } = useI18n();
  const nav = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isAdmin, setIsAdmin] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const createMut = useMutation({
    mutationFn: async () => {
      const body: CreateUserReq = {
        email: email.trim(),
        password,
        isAdmin,
      };
      const r = await postApi<{ id: string }>("/admin/users", body);
      return r.data;
    },
    onSuccess: (data) => nav(`/users/${data.id}`),
    onError: (e) => setError(e instanceof Error ? e.message : String(e)),
  });

  return (
    <Page
      title="New user"
      description="Create a new platform user"
      actions={
        <Button onClick={() => createMut.mutate()} disabled={createMut.isPending}>
          {createMut.isPending ? "Creating..." : "Create"}
        </Button>
      }
    >
      {error && (
        <div className="mb-4 rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
          {error}
        </div>
      )}

      <div className="max-w-xl space-y-4 rounded-2xl border p-4">
        <div className="space-y-2">
          <Label>Email</Label>
          <Input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="user@example.com" />
        </div>

        <div className="space-y-2">
          <Label>Temporary password</Label>
          <Input value={password} onChange={(e) => setPassword(e.target.value)} type="password" placeholder="••••••••" />
          <div className="text-xs text-muted-foreground">
            Sonra “reset password” flow ekleriz.
          </div>
        </div>

        <div className="flex items-center justify-between rounded-xl border p-3">
          <div>
            <div className="text-sm font-medium">Admin</div>
            <div className="text-xs text-muted-foreground">Grants access to admin console.</div>
          </div>
          {/* <Switch checked={isAdmin} onCheckedChange={setIsAdmin} /> */}
        </div>
      </div>
    </Page>
  );
}
