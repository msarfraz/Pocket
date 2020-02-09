function createJQGrid(listurl, colNames, colModel, pkey, editurl, caption, edrowid, prowid, postData, extraParams, subGrid, subGridRowExpanded, subGridRowColapsed) {
    
    if (!postData)
        postData = {};
    if (!extraParams)
        extraParams = {};
    // set defaults
    postData["__RequestVerificationToken"] = jQuery('input[name=__RequestVerificationToken]').val();
    if (!extraParams['sidx'])
        extraParams['sidx'] = pkey;
    if (!extraParams['sord'])
        extraParams['sord'] = 'asc';
    if (!'addgrid' in extraParams)
        extraParams['addgrid'] = true;
    if (!'editgrid' in extraParams)
        extraParams['editgrid'] = true;
    if (!'gridview' in extraParams)
        extraParams['gridview'] = true;
    if (!'treeGrid' in extraParams)
        extraParams['treeGrid'] = false;
    if (!extraParams['editExtraParams'])
        extraParams['editExtraParams'] = {};
    extraParams['editExtraParams']["__RequestVerificationToken"] = jQuery('input[name=__RequestVerificationToken]').val();

    //create grid
    jQuery(edrowid).jqGrid({
        url: listurl,
        postData: postData,
        datatype: "json",
        height: 'auto',
        autowidth: true,
        forceFit: true,
        colNames: colNames,
        colModel: colModel,
        keyIndex: true,
        rowNum: 10,
        rowList: [10,20,50,100],
        pager: prowid,
        sortname: extraParams['sidx'],
        viewrecords: true,
        sortorder: extraParams['sord'],
    //    cmTemplate: { editable: true },
        prmNames: { addoper: 'create', deloperator: 'delete', id: pkey },
        gridview: extraParams['gridview'],
        ignoreCase: true,
        rownumbers: true,
        editurl: editurl,
        caption: caption,
        gridComplete: function () { var ids = jQuery(edrowid).jqGrid('getDataIDs'); },
        subGrid: subGrid,
        //subGridUrl : fuse1,
        //subGridModel:fuse2,
        subGridRowExpanded: subGridRowExpanded,
        subGridRowColapsed: subGridRowColapsed,
        grouping: extraParams['grouping'],
        groupingView: { groupField: extraParams['groupField'],groupOrder:extraParams['groupOrder'],hideFirstGroupCol:true,showSummaryOnHide:true, groupDataSorted: true, groupSummary: [true], groupSummaryPos: ['footer'], showSummaryOnHide: true }
        
           
    });
    //jQuery(edrowid).jqGrid('filterToolbar', { stringResult: true, searchOnEnter: false, defaultSearch: "ExpenseDate" });
    jQuery(edrowid).jqGrid('navGrid', prowid, { edit: false, add: false, del: false,search:false });
    jQuery(edrowid).jqGrid('inlineNav', prowid, {
        edit: extraParams['editgrid'],
        add: extraParams['addgrid'],
        addParams: {
            position: "first", //afterSelected,last,beforeSelected
            rowID: '0',
            useDefValues: true,
            addRowParams: {
                keys: true,
                extraparam: extraParams['editExtraParams'],
                //onSuccess: function (jqXHR) {
                //    alert('onSuccess');
                //    jqXHRFromOnSuccess = jqXHR;
                //    return true;
                //},
                //afterSave: function (rowID) {
                //    alert('afterSave');
                //    cancelEditing($grid);
                //    afterDetailSaveFunc(rowID, jqXHRFromOnSuccess);
                //    jqXHRFromOnSuccess = null;
                //},
                //onError: function () { alert('onError'); },
                successfunc: function (response, id) {
                    $(edrowid).jqGrid().trigger('reloadGrid');
                    return response.responseJSON.success;
                }
            }
            
        },
        editParams: {
            keys: true,
            extraparam: extraParams['editExtraParams'], //{ __RequestVerificationToken: jQuery('input[name=__RequestVerificationToken]').val() },
            successfunc: function (response, id) {
                //$("#edrowid").jqGrid('setGridParam', { datatype: 'json' });
                if(!response.responseJSON.success)
                    $(edrowid).jqGrid().trigger('reloadGrid');
                return response.responseJSON.success;
            }
        }
    });

}

function CreateJQGridSubGrid(listurl, colNames, colModel, pkey, editurl, caption, edrowid, prowid, sgUrl, sgColNames, sgColModel, sgPKey, sgEditUrl, sgCaption, extraParams) {

    subGridRowExpanded= function(subgrid_id, row_id) {
        // we pass two parameters
        // subgrid_id is a id of the div tag created whitin a table data
        // the id of this elemenet is a combination of the "sg_" + id of the row
        // the row_id is the id of the row
        // If we wan to pass additinal parameters to the url we can use
        // a method getRowData(row_id) - which returns associative array in type name-value
        // here we can easy construct the flowing
        var subgrid_table_id, pager_id;
        subgrid_table_id = subgrid_id+"_t";
        pager_id = "p_"+subgrid_table_id;
        $("#"+subgrid_id).html("<table id='"+subgrid_table_id+"' class='scroll'></table><div id='"+pager_id+"' class='scroll'></div>");

        //create subgrid
        createJQGrid(sgUrl + row_id, sgColNames, sgColModel, sgPKey, sgEditUrl + row_id, sgCaption, "#" + subgrid_table_id, "#" + pager_id, null, {sidx:extraParams['sg_sidx'], sord:extraParams['sg_sord']});
        
    };
    subGridRowColapsed= function(subgrid_id, row_id) {
        // this function is called before removing the data
        var subgrid_table_id;
        subgrid_table_id = subgrid_id+"_t";
        jQuery("#"+subgrid_table_id).remove();
    }

    //create main grid
    createJQGrid(listurl, colNames, colModel, pkey, editurl, caption, edrowid, prowid, null, extraParams, true, subGridRowExpanded, subGridRowColapsed);

}

function CreateJQGridTree(gridid, listurl, colNames, colModel, ExpandColumn, caption, postData, sidx)
{
    $(gridid).jqGrid({
        url: listurl,
        datatype: 'json',
        mtype: 'GET',
        colNames: colNames,
        colModel: colModel,
        treeGridModel: 'adjacency',
        height: 'auto',
        rowNum: 10000,
        treeGrid: true,
        ExpandColumn: ExpandColumn,
        caption: caption,
        postData: postData,
        sortname: sidx
    });
}