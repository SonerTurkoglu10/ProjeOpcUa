var intervalId = null;

function fetchData() {
    var nodeId = $('#nodeId').val();
    var namespaceIndex = $('#namespaceIndex').val();

    $.ajax({
        url: `https://localhost:7046/api/OpcData/GetOpcData?nodeId=${nodeId}&namespaceIndex=${namespaceIndex}`,
        type: 'GET',
        success: function (data) {
            console.log(data); // Konsola gelen veriyi yazdırma

            // Gelen veriyi tabloya ekleyelim
            updateTable(nodeId, data);

            // Hata mesajını temizle
            $('#error-message').text('');
        },
        error: function (xhr, status, error) {
            console.error("Hata:", xhr.responseText);

            // Hata mesajını göster ve veri çekmeyi durdur
            $('#error-message').text('Veri çekme işlemi sırasında bir hata oluştu.');
            stopFetchingData();
        }
    });
}

function cleanValue(value) {
    // Sadece değeri döndür
    if (typeof value === 'object' && value !== null && value.hasOwnProperty('dataValue')) {
        return value.dataValue; // Sadece veri değeri döndürülüyor
    }
    return value; // Eğer string ya da başka bir veri türüyse, olduğu gibi döndür
}

function updateTable(nodeId, data) {
    var value = cleanValue(data);

    // Tabloyu güncelleme
    var existingRow = $(`#opcDataTableBody tr[data-node-id="${nodeId}"]`);
    if (existingRow.length > 0) {
        // Var olan satırı güncelle
        existingRow.find('td').eq(1).text(value);
    } else {
        // Yeni satırı tabloya ekleme
        var row = `<tr data-node-id="${nodeId}">
                        <td>${nodeId}</td>
                        <td>${value}</td>
                        <td>
                            <button class="btn btn-warning btn-sm edit-btn">Güncelle</button>
                        </td>
                    </tr>`;
        $('#opcDataTableBody').append(row);
    }
}

function startFetchingData() {
    if (!intervalId) {
        fetchData(); // İlk veriyi hemen çek
        intervalId = setInterval(fetchData, 1000); // Her saniye veri çek
    }

    $('#getDataBtn').prop('disabled', true); // Başlatma butonunu devre dışı bırak
    $('#stopDataBtn').prop('disabled', false); // Durdurma butonunu etkinleştir
}

function stopFetchingData() {
    if (intervalId) {
        clearInterval(intervalId);
        intervalId = null;

        $('#getDataBtn').prop('disabled', false); // Başlatma butonunu etkinleştir
        $('#stopDataBtn').prop('disabled', true); // Durdurma butonunu devre dışı bırak
    }
}

$(document).ready(function () {
    $('#getDataBtn').click(function () {
        startFetchingData();
    });

    $('#stopDataBtn').click(function () {
        stopFetchingData();
    });

    // Sayfa yüklendiğinde durdurma butonunu devre dışı bırak
    $('#stopDataBtn').prop('disabled', true);

    // Güncelle butonuna tıklama olayını yakala
    $(document).on('click', '.edit-btn', function () {
        var $row = $(this).closest('tr');
        var nodeId = $row.data('node-id');
        var currentValue = $row.find('td').eq(1).text();

        // Güncelleme formunu aç
        $('#updateNodeId').val(nodeId);
        if (currentValue === 'true' || currentValue === 'false') {
            $('#updateValue').hide();
            $('#booleanOptions').show();
            $('#booleanValue').val(currentValue);
        } else {
            $('#booleanOptions').hide();
            $('#updateValue').show();
            $('#updateValue').val(currentValue);
        }

        $('#updateModal').modal('show');
        
        // Güncelleme formunun gönderilme işlemi
        $('#updateForm').off('submit').on('submit', function (event) {
            event.preventDefault();
            var newValue;
            if ($('#booleanOptions').is(':visible')) {
                newValue = $('#booleanValue').val();
            } else {
                newValue = $('#updateValue').val();
            }

            // API'ye güncellenmiş veriyi gönder
            $.ajax({
                url: `https://localhost:7046/api/OpcData/UpdateOpcData`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    nodeId: nodeId,
                    newValue: newValue,
                    namespaceIndex: $('#namespaceIndex').val() // Güncellenmiş namespaceIndex'i de gönder
                }),
                success: function () {
                    // Modalı kapat ve tablodaki veriyi güncelle
                    $('#updateModal').modal('hide');
                    fetchData(); // Güncellenmiş veriyi yeniden çek
                },
                error: function (xhr, status, error) {
                    console.error("Güncelleme hatası:", xhr.responseText);
                }
            });
        });
    });
});
