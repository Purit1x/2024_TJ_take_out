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

<!-- <template>
    <div>
        <h2>评论管理</h2>
        <div class="order-item" v-for="(orderItem, index) in finishedOrders" :key="index">
            订单号：{{ orderItem.orderId }}
            评论：{{ displayOrdersComment(orderItem.orderId) }}
        </div>
        <button @click="gobackHome">返回</button>
    </div>
</template> -->


<template>
    <div>
        <h2>评论管理</h2>
        <button @click="gobackHome">返回</button>
        <el-table :data="finishedOrders" style="width:100%">
            <el-table-column label="订单号" width="180">
                <template #default="scope">
                    <div style="display: flex; align-items: center">
                        
                        <span style="margin-left: 10px">{{ scope.row.orderId }}</span>
                    </div>
                </template>
            </el-table-column>

            <el-table-column label="评论" width="180">
                <template #default="scope">
                    <div style="display: flex; align-items: center">
                        
                        <span style="margin-left: 10px">{{ displayOrdersComment(scope.row.orderId) }}</span>
                    </div>
                </template>
            </el-table-column>

            <el-table-column label="操作">
                <template #default="scope">
                    <el-button
                        size="small"
                        type="danger"
                        @click="handleDelete(scope.row.orderId)"
                    >
                    删除评论
                    </el-button>
                </template>
            </el-table-column>
        </el-table>
    </div>
</template>