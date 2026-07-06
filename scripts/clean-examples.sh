#!/usr/bin/env bash
set -euo pipefail

# clean-examples.sh
#
# Removes all commented-out InvoiceAddress example files from a scaffolded
# project. Run this after you've replaced the example code with your own
# production logic.
#
# Run from the project root:
#   bash scripts/clean-examples.sh

echo "=== Removing InvoiceAddress example files ==="

EXCLUDE=( -not \( -path "*/bin/*" -o -path "*/obj/*" -o -path "*/.git/*" \) )

echo ""
echo "--- Deleting InvoiceAddress example directories ---"
find . -type d "${EXCLUDE[@]}" \( \
    -path "*/InvoiceAddresses" -o \
    -path "*/InvoiceAdresses" \
\) -print 2>/dev/null | while IFS= read -r dir; do
    rm -rf "$dir"
    echo "  removed: $dir/"
done

echo ""
echo "--- Deleting InvoiceAddress example files ---"
find . -type f "${EXCLUDE[@]}" \( \
    -name "InvoiceAddress.cs" -o \
    -name "InvoiceAddressTests.cs" -o \
    -name "InvoiceAddressEntityConfiguration.cs" -o \
    -name "InvoiceAddressCreatedEvent.cs" -o \
    -name "CreateInvoiceAddressCommandHandler.cs" -o \
    -name "CreateInvoiceAddressCommandValidatorTests.cs" -o \
    -name "CreateInvoiceAddressCommandHandlerTests.cs" -o \
    -name "CreateInvoiceAddressRequest.cs" -o \
    -name "CreateInvoiceAddressTests.cs" -o \
    -name "UpdateInvoiceAddressCommandHandler.cs" -o \
    -name "UpdateInvoiceAddressCommandValidatorTests.cs" -o \
    -name "UpdateInvoiceAddressCommandHandlerTests.cs" -o \
    -name "UpdateInvoiceAddressRequest.cs" -o \
    -name "UpdateInvoiceAddressTests.cs" -o \
    -name "GetInvoiceAddressByIdQueryHandler.cs" -o \
    -name "GetInvoiceAddressByIdQueryHandlerTests.cs" -o \
    -name "GetInvoiceAddressByIdTests.cs" -o \
    -name "GetInvoiceAddressesQueryHandler.cs" -o \
    -name "GetInvoiceAddressesQueryHandlerTests.cs" -o \
    -name "GetInvoiceAddressesRequest.cs" -o \
    -name "GetInvoiceAddressesTests.cs" -o \
    -name "Address.cs" -o \
    -name "AddressTests.cs" -o \
    -name "AddressRequest.cs" \
\) -print 2>/dev/null | while IFS= read -r file; do
    rm "$file"
    echo "  removed: $file"
done

echo ""
echo "--- Cleaning up empty directories ---"
find . -type d -empty -not -path "./.git/*" -not -path "./.git" \
    -not -path "./.template.config/*" -delete 2>/dev/null || true

echo ""
echo "=== Done. InvoiceAddress example files have been removed. ==="
