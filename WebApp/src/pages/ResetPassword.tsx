import { useMemo, useState } from "react";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { postApi } from "@/lib/api";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Link, useLocation, useNavigate } from "react-router-dom";

const schema = z.object({
  password: z.string().min(8, "Min 8 characters"),
});

type FormData = z.infer<typeof schema>;

function useQueryParam(name: string) {
  const { search } = useLocation();
  return useMemo(() => new URLSearchParams(search).get(name), [search, name]);
}

export function ResetPassword() {
  const token = useQueryParam("token");
  const nav = useNavigate();

  const [error, setError] = useState<string | null>(null);
  const { register, handleSubmit, formState: { isSubmitting, errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { password: "" },
  });

  const onSubmit = async (data: FormData) => {
    setError(null);
    try {
      if (!token) throw new Error("Missing token");
      await postApi("/auth/reset-password", { token, newPassword: data.password });
      nav("/login", { replace: true });
    } catch (e) {
      setError(e instanceof Error ? e.message : "Reset failed");
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <Card className="rounded-2xl">
          <CardHeader>
            <CardTitle>Reset password</CardTitle>
            <CardDescription>Set a new password for your account.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {!token && (
              <div className="rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
                Missing token. Please use the link from your email.
              </div>
            )}

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="password">New password</Label>
                <Input id="password" type="password" autoComplete="new-password" {...register("password")} />
                {errors.password && <p className="text-sm text-destructive">{errors.password.message}</p>}
              </div>

              {error && (
                <div className="rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
                  {error}
                </div>
              )}

              <Button className="w-full" disabled={isSubmitting || !token}>
                {isSubmitting ? "Saving..." : "Reset password"}
              </Button>

              <div className="text-sm text-muted-foreground text-center">
                <Link className="hover:underline" to="/login">Back to login</Link>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
