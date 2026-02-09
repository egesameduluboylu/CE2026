import { useState } from "react"
import { useQuery } from "@tanstack/react-query"
import { Input } from "@/components/ui/input"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Button } from "@/components/ui/button"

type FetchParams = {
  search: string
  page: number
  pageSize: number
}

type PageResult<T> = {
  items: T[]
  total: number
  page: number
  pageSize: number
}

type Column<T> = {
  header: string
  cell: (row: T) => React.ReactNode
}

type Props<T> = {
  queryKey: string[]
  fetcher: (params: FetchParams) => Promise<PageResult<T>>
  columns: Column<T>[]
}

export function DataTable<T>({
  queryKey,
  fetcher,
  columns,
}: Props<T>) {
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const pageSize = 20

  const { data, isLoading, isError, error } = useQuery({
    queryKey: [...queryKey, search, page, pageSize],
    queryFn: () => fetcher({ search, page, pageSize }),
  })

  const totalPages = data ? Math.ceil(data.total / pageSize) : 0

  return (
    <div className="space-y-4">
      {/* Search */}
      <div className="flex justify-between items-center">
        <Input
          placeholder="Search..."
          className="w-[260px]"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value)
            setPage(1)
          }}
        />
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="text-sm text-muted-foreground">Loading…</div>
      )}

      {/* Error */}
      {isError && (
        <div className="text-sm text-destructive">
          {error instanceof Error ? error.message : String(error)}
        </div>
      )}

      {/* Table */}
      {!isLoading && !isError && (
        <>
          <div className="rounded-2xl border overflow-hidden">
            <Table>
              <TableHeader>
                <TableRow>
                  {columns.map((col, i) => (
                    <TableHead key={i}>{col.header}</TableHead>
                  ))}
                </TableRow>
              </TableHeader>

              <TableBody>
                {data?.items?.length ? (
                  data.items.map((row, rowIndex) => (
                    <TableRow key={rowIndex}>
                      {columns.map((col, colIndex) => (
                        <TableCell key={colIndex}>
                          {col.cell(row)}
                        </TableCell>
                      ))}
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell
                      colSpan={columns.length}
                      className="py-10 text-center text-sm text-muted-foreground"
                    >
                      No data found.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex justify-between items-center">
              <div className="text-sm text-muted-foreground">
                Page {page} of {totalPages}
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
    </div>
  )
}
