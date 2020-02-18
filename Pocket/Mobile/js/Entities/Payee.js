define(['app'], function (app) {


    function MList(database, data, callback) {
        database.transaction(
            function (transaction) {

                transaction.executeSql(
                    (
                        "SELECT " +
                            "* " +
                        "FROM " +
                            "payees " +
                        "ORDER BY " +
                            "name ASC"
                    ),
                    [],
                    function (transaction, results) {
                        callback(results);
                    }
                );

            }
        );
    }
    function MEdit(database, data, callback) {
        database.transaction(
            function (transaction) {
                if (data.PayeeID == 0)
                    query = "INSERT INTO payees (name) VALUES ( ? );";
                else
                    query = "UPDATE payees set Name = ? where PayeeID = ?";

                transaction.executeSql(
                    (
                        query
                    ),
                    [data.PayeeID, data.Name],
                    function (transaction, results) {
                        callback(database.success());
                    }
                );

            }
        );
    }
    function MDelete(database, data, callback) {
        database.transaction(
            function (transaction) {
                try {
                    transaction.executeSql(
                    (
                        "DELETE payees where PayeeID = ?"
                    ),
                    [data.PayeeID],
                    function (transaction, results) {
                        callback(database.success());
                    }
                );
                }
                catch (err) {
                    document.getElementById("demo").innerHTML = err.message;
                }
                

            }
        );
    }

    return {
        MList: MList
    };
});