<script setup>
import { useStore } from "vuex";
import { ElMessage ,ElMessageBox} from "element-plus";
import { ref, onMounted, watch, onBeforeMount, onBeforeUnmount } from 'vue';  
import { useRouter } from 'vue-router';
import { getFinishedOrders,getFinishedOrdersComment ,deleteOrdersComment} from "@/api/platform";
const store = useStore();
const router = useRouter();
const gobackHome = () => {
    router.push('/platform-home');
}
const finishedOrders=ref([]);
const ordersComment=ref([]);
let updateInterval=null;
onMounted(async()=>{
    await renewOrders();
    updateInterval=setInterval(renewOrdersComment,1000);
})
onBeforeUnmount(()=>{
    if (updateInterval) {
        clearInterval(updateInterval); // 清除定时器  
    }
})
const renewOrders=async()=>{
    try{
        const finishedOrderData=await getFinishedOrders();
        if(finishedOrderData===0){
            ElMessage.success('无已完成订单');
            finishedOrders.value=[];
        }
        else{
            finishedOrders.value=finishedOrderData.data;
        }
        console.log('orders',finishedOrders.value);
    }catch(error)
    {
        throw error;
    }
}
const renewOrdersComment=async()=>{
    if (finishedOrders.value.length > 0) {
        // 创建一个Promise数组，同时加载所有订单的评论
        const promise1 = finishedOrders.value.map(orderItem => getFinishedOrdersComment(orderItem.orderId));

        // 等待所有异步请求完成
        const fees1 = await Promise.all(promise1);

        for (let i = 0; i < fees1.length; i++) {
            ordersComment.value[finishedOrders.value[i].orderId] = fees1[i];
        }
    }
    console.log('comment',ordersComment.value);
}
const handleDelete=async(Id)=>{
    ElMessageBox.confirm('确认要删除该评论吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
    }).then(()=>{
        deleteOrdersComment(Id).then(res=>{
            ElMessage.success('删除成功');
            ordersComment.value[Id]=null;
        }).catch(err => {
            ElMessage.error('删除失败');
            console.log("删除异常",err);
        });
    })

}


function displayOrdersComment(orderId) {
    return ordersComment.value[orderId] ||'无评论';
}

</script>




<template>
    <div class="box">
        <div class="head">评论管理</div>
        <el-table :data="finishedOrders"  class="table" border>
            <el-table-column label="订单号" width="180" align="center">
                <template #default="scope" align="center">
                    <div style="display: flex; align-items: center; justify-content: center;" class="comment">
                        <span style="margin-left: 10px" align="center">{{ scope.row.orderId }}</span>
                    </div>
                </template>
            </el-table-column>

            <el-table-column label="评论" width="540" align="center">
                <template #default="scope">
                    <div style="display: flex; align-items: center; justify-content: center;" class="comment">
                        <span style="margin-left: 10px;">{{ displayOrdersComment(scope.row.orderId) }}</span>
                    </div>
                </template>
            </el-table-column>

            <el-table-column label="操作" align="center">
                <template #default="scope" align="center">
                    <el-button size="small" type="danger" @click="handleDelete(scope.row.orderId)">
                        删除评论
                    </el-button>
                </template>
            </el-table-column>
        </el-table>
        <button @click="gobackHome" class="return">返回</button>
    </div>
</template>
<style scoped lang="scss">
.comment{
    display: flex;
    width:100%;
    text-align: center;
}
.table{
    margin-top:10px;
    margin-left:5%;
    height:80%;
    width:90%;
    border-radius: 20px;
    border: 2px solid #01042a;
    table-layout: auto;
    overflow: auto;
}

.box{
    padding: 20px;
    background-color: #7ac2ee;
    border: 2px solid #000000;
    border-radius: 20px;
    margin-right: 30px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);

    font-size: 3vmin; /* 字体大小 */
    position: fixed; /* 固定定位 */
    top: 60px; /* 贴近顶部 */
    left: 50%; /* 水平居中 */
    transform: translateX(-50%); /* 修正水平居中 */
    width: 70%;
    height: 85%;

    .el-table {
        max-height: 100%; // 确保表格的最大高度不超过其父元素
        overflow-y: auto; // 表格内部也启用滚动条
    }
}

.return{
    padding: 10px 15px;
    position: absolute;
    right: 10px;
    bottom: 10px;
    background-color: #FFC0CB;
}
.return:hover{
    background-color: #f7ced5;
}
.head{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    left:50%;
    font-size: 4vmin; /* 字体大小 */
    color:#000000;
}

.input{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    margin-left:30%;
    margin-right:30%;
    width:40%;
    font-size: 3vmin;
}
</style>