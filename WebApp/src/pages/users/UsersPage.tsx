import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { getApi } from "@/lib/api";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Page } from "@/shared/components/Page";
import { Badge } from "@/components/ui/badge";

type UserItem = {
  id: string;
  email: string;
  createdAt: string;
  isAdmin: boolean;
  failedLoginCount: number;
  lockoutUntil: string | null;
};

type PageResult = {
  items: UserItem[];
  total: number;
  page: number;
  pageSize: number;
};

export function UsersPage() {
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 20;

  const queryKey = useMemo(() => ["admin", "users", search, page, pageSize], [search, page, pageSize]);

  const { data, isLoading, isError, error } = useQuery({
    queryKey,
    queryFn: async () => {
      const r = await getApi<PageResult>(
        `/admin/users?search=${encodeURIComponent(search)}&page=${page}&pageSize=${pageSize}`
      );
      return r.data as PageResult;
    },
  });

  const totalPages = data ? Math.ceil(data.total / pageSize) : 0;

  return (
    <Page
      title="Users"
      description="Manage platform users"
      actions={
        <div className="flex gap-2">
          <div className="w-[260px]">
            <Input
              type="search"
              placeholder="Search by email…"
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setPage(1);
              }}
            />
          </div>
          <Button asChild>
            <Link to="/users/new">New user</Link>
          </Button>
        </div>
      }
    >
      {/* states */}
      {isLoading && (
        <div className="text-sm text-muted-foreground">Loading…</div>
      )}

      {isError && (
        <div className="rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
          {error instanceof Error ? error.message : String(error)}
        </div>
      )}

      {!isLoading && !isError && (
        <>
          <div className="rounded-2xl border overflow-hidden">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Email</TableHead>
                  <TableHead>Created</TableHead>
                  <TableHead>Admin</TableHead>
                  <TableHead>Failed logins</TableHead>
                  <TableHead>Lockout</TableHead>
                  <TableHead className="text-right">Action</TableHead>
                </TableRow>
              </TableHeader>

              <TableBody>
                {data?.items?.length ? (
                  data.items.map((u) => (
                    <TableRow key={u.id}>
                      <TableCell className="font-medium">{u.email}</TableCell>
                      <TableCell className="text-muted-foreground">
                        {new Date(u.createdAt).toLocaleString()}
                      </TableCell>
                      <TableCell>
                        {u.isAdmin ? (
                          <Badge>Admin</Badge>
                        ) : (
                          <Badge variant="secondary">User</Badge>
                        )}
                      </TableCell>
                      <TableCell>{u.failedLoginCount}</TableCell>
                      <TableCell className="text-muted-foreground">
                        {u.lockoutUntil
                          ? new Date(u.lockoutUntil).toLocaleString()
                          : "—"}
                      </TableCell>
                      <TableCell className="text-right">
                        <Button variant="ghost" size="sm" asChild>
                          <Link to={`/users/${u.id}`}>View</Link>
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell colSpan={6} className="py-10 text-center text-sm text-muted-foreground">
                      No users found.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>

          {/* pagination */}
          {totalPages > 1 && (
            <div className="mt-4 flex items-center justify-between gap-3">
              <div className="text-sm text-muted-foreground">
                Page <span className="font-medium text-foreground">{page}</span>{" "}
                of <span className="font-medium text-foreground">{totalPages}</span>
              </div>

              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={page <= 1}
                  onClick={() => setPage((p) => p - 1)}
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={page >= totalPages}
                  onClick={() => setPage((p) => p + 1)}
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </>
      )}
    </Page>
  );
}
