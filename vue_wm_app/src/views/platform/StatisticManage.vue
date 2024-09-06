<script setup>
    import { ElMessage } from "element-plus";
    import { ref } from 'vue';  
    import { getQuantityByRegion,getSalesByRegion } from '@/api/platform';
    import { useRouter } from 'vue-router';
    const router = useRouter();
    const region_c = ref('');
    const region_s = ref('');
    const orderCount = ref(null);
    const sales = ref(null);
    const gobackHome = () => {
        router.push('/platform-home');
    }
    const fetchOrderCount = async () => {
        try {
            const response = await getQuantityByRegion(region_c.value);
            console.log('Processed response:', response); // 增加日志输出以验证处理后的数据
            orderCount.value = response.orderCount;
            region_c.value = response.region; // 确保也更新了区域名称
            //ElMessage.success(`区域 ${response.region} 的订单量为: ${response.orderCount}`);
        } catch (error) {
            console.error('Error fetching order count:', error);
            ElMessage.error('查询失败');
        }
    };
    const fetchSales = async () => {
        try {
            const response = await getSalesByRegion(region_s.value);
            console.log('Processed response:', response.totalSales); // 增加日志输出以验证处理后的数据
            sales.value = response.totalSales;
            region_s.value = response.region; // 确保也更新了区域名称
            //ElMessage.success(`区域 ${response.region} 的为: ${response.totalSales}`);
        } catch (error) {
            console.error('Error fetching order count:', error);
            ElMessage.error('查询失败');
        }
    };
</script>
<template>
    <div class ="box">
        <div class = "head">数据统计</div>

        <div>
            <div class="head2">按照区域查询订单量:</div>
            <div class="input">
                <input v-model="region_c" placeholder="输入区域名称">
                <button @click="fetchOrderCount" class="choose">查询订单量</button>
                
            </div>
            <div v-if="orderCount !== null"class="output">
                区域 {{ region_c }} 的订单量: {{ orderCount }}
            </div>
        </div>

        <div>
            <div class="head2">按照区域查询销售额:</div>
            <div class="input">
            <input v-model="region_s" placeholder="输入区域名称">
            <button @click="fetchSales" class="choose">查询销售额</button>
            </div>
            <div v-if="sales !== null" class="output">
                区域 {{ region_s }} 的销售额: {{ sales }}
            </div>
        </div>

        <button @click="gobackHome" class ="return">返回</button>
    </div>
</template>
<style scoped lang="scss">
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
    height: 70%;
}

.return{
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
    font-size: 4.5vmin; /* 字体大小 */
    color:#000000;

}
.choose{
    padding: 5px 8px;         /* 按钮内边距 */
    margin-right: 8px;         /* 按钮右边距 */
    background-color: #f68396;
    font-size: 2.5vmin; /* 字体大小 */
}
.choose:hover{
    background-color: #FFC0CB;
}
.head2{
    margin-top:4%;
    margin-bottom: 3%;
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    color:#54141f;
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
.output{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    margin-top:1%;
   // width:100%;
    font-size: 3vmin;
    color:#583def;
}
</style>