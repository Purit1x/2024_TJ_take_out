<script setup>
    import { useStore } from "vuex";
    import { ElMessage,ElMessageBox } from "element-plus";
    import { ref, onMounted, computed } from 'vue';  
    import { getStationIds, getStationsInfo,getQuantityByRegion,getSalesByRegion,stationDeleteService,updateStation,addStation} from '@/api/platform';
    import { getMerchantIds,getMerchantsInfo} from '@/api/user';
    import { EditMerchantStation, assignStationToMerchant,AssignStation } from '@/api/merchant';
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
            ElMessage.success(`区域 ${response.region} 的订单量为: ${response.orderCount}`);
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
            ElMessage.success(`区域 ${response.region} 的为: ${response.totalSales}`);
        } catch (error) {
            console.error('Error fetching order count:', error);
            ElMessage.error('查询失败');
        }
    };
</script>
<template>
    <div>
        片区管理
        <button @click="gobackHome">返回</button>
    </div>
    <!-- 添加区域订单量查询 -->
    <div>
        按照区域查询订单量
        <input v-model="region_c" placeholder="输入区域名称">
        <button @click="fetchOrderCount">查询订单量</button>
        <p v-if="orderCount !== null">
            区域 {{ region_c }} 的订单量: {{ orderCount }}
        </p>
    </div>
    <div>
        按照区域查询销售额
        <input v-model="region_s" placeholder="输入区域名称">
        <button @click="fetchSales">查询销售额</button>
        <p v-if="sales !== null">
            区域 {{ region_s }} 的销售额: {{ sales }}
        </p>
    </div>
</template>