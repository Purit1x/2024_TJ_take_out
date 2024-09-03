<script setup>
import { useStore } from "vuex";
import { ElMessage,ElMessageBox } from "element-plus";
import { ref, onMounted } from 'vue';  
import { useRouter } from 'vue-router';
import { getMerchantIds,getMerchantsInfo } from "@/api/user";
import { merchantDeleteService } from "@/api/platform";
import { getMerAvgRating } from "@/api/merchant";
const router = useRouter();
const merchantIds = ref([]);  //获取所有商家id
const merchantsInfo = ref([]); // 存储所有商家信息 
const showMerchantsInfo = ref([]); // 显示商家信息列表
const searchQuery = ref(''); // 搜索框内容
const isMerchantInfo= ref(false); // 是否显示商家信息
const currentMerchantId = ref(null); // 当前商家id
const MerchantInfo=ref({});  //商户信息
const parentBorder=ref(false);
const childBorder=ref(false);

onMounted(async() => {
    await getMerchantIds().then(res => {  // 获取所有商家id
        merchantIds.value = res.data;  
        return Promise.all(merchantIds.value.map(id => getMerchantsInfo(id))); // 并发请求所有商家信息
    }).then(responses => {  
        merchantsInfo.value = responses.map(response => response.data); // 提取商家信息  
        showMerchantsInfo.value = merchantsInfo.value; // 显示商家信息列表
    }).catch(err => {  
        ElMessage.error('获取商家id失败'); 
    }); 
    await fetchMerAvgRating();
});
const fetchMerAvgRating = async () => {
  try {
    for (let info of showMerchantsInfo.value) {
      const avgRating = await getMerAvgRating(info.merchantId);
      console.log(`Merchant ID: ${info.merchantId}, Avg Rating: ${avgRating}`);
      info.avgRating = avgRating; // 将平均评分添加到商家信息对象中
    }
    console.log("商家信息", showMerchantsInfo.value);
  } catch (error) {
    console.error('Failed to fetch merchant average ratings:', error);
  }
};
const formatTime=(seconds)=> {  
    const hours = Math.floor(seconds / 3600);  
    const minutes = Math.floor((seconds % 3600) / 60);  
    const secs = seconds % 60;  
    return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;  
}  
const handleSearch = () => {   //字符串匹配搜索商家
    const query = searchQuery.value.trim();  
    if (query) {  
        showMerchantsInfo.value = merchantsInfo.value.filter(merchant =>  
            merchant.merchantName.includes(query) ||  
            merchant.dishType.includes(query)  
        );  
    } else {  
        showMerchantsInfo.value = merchantsInfo.value;  
    }
};  
const enterMerchantInfo = (merchantId) => {  // 进入商家管理页面
    currentMerchantId.value = merchantId;
    MerchantInfo.value = merchantsInfo.value.find(merchant => merchant.merchantId === merchantId);
    MerchantInfo.value.timeforOpenBusiness=formatTime(MerchantInfo.value.timeforOpenBusiness);  //时间转换
    MerchantInfo.value.timeforCloseBusiness=formatTime(MerchantInfo.value.timeforCloseBusiness);
    isMerchantInfo.value = true;
};
const deleteMerchant = (merchantId) => {  // 销号
    ElMessageBox.confirm('确认要删除该商家吗?', '提示', {  
        confirmButtonText: '确定',  
        cancelButtonText: '取消',  
        type: 'warning'  
    }).then(() => {  
        merchantDeleteService(merchantId).then(res => {  
            ElMessage.success('销号成功');  
            showMerchantsInfo.value = showMerchantsInfo.value.filter(merchant => merchant.merchantId!== merchantId);  
            isMerchantInfo.value = false;  
            currentMerchantId.value = null;  
            MerchantInfo.value = {};  
        }).catch(err => {  
            ElMessage.error('销号失败');  
        });  
    }).catch(() => {});  
};
const gobackHome = () => {
    router.push('/platform-home');
}
</script>

<!-- <template>
    <div v-if="!isMerchantInfo">
        <div>
            商家管理
            <button @click="gobackHome">返回</button>
        </div>
        <h2>商家列表</h2>  
        <div>
            <input type="text" v-model="searchQuery" placeholder="搜索店名或类别" v-on:keyup.enter="handleSearch()"/> 
            <button @click="handleSearch()">搜索</button>
        </div>
        <ul>  
            <li v-for="merchant in showMerchantsInfo" :key="merchant.merchantId">  
                <span>{{ merchant.merchantName }}</span> 
                <span>&nbsp;&nbsp;{{ merchant.dishType }}</span>
                <span>&nbsp;<button @click="enterMerchantInfo(merchant.merchantId)">></button></span>
            </li>  
        </ul> 
    </div> 
    <div v-if="isMerchantInfo">
        <div>商家信息</div>
        <div>地址：{{MerchantInfo.merchantAddress}}</div>
        <div>营业时间：{{ MerchantInfo.timeforOpenBusiness }} - {{ MerchantInfo.timeforCloseBusiness }}</div>
        <div>联系电话：{{MerchantInfo.contact}}</div>
        <div>是否可以使用通用优惠券：{{MerchantInfo.couponType ? '否' : '是'}}</div>
        <div>
            <button @click="deleteMerchant(currentMerchantId)">销号</button>
            <button @click="isMerchantInfo=false">返回</button>
        </div>
    </div>
</template> -->




<template>
    <div v-if="!isMerchantInfo">
        <div>
            商家管理
            <button @click="gobackHome">返回</button>
        </div>
        <h2>商家列表</h2>  
        <div>
            <input type="text" v-model="searchQuery" placeholder="搜索店名或类别" v-on:keyup.enter="handleSearch()"/> 
            <button @click="handleSearch()">搜索</button>
        </div>
    
            <el-table :data="showMerchantsInfo":border="parentBorder"style="width:100%">
            <el-table-column type="expand">
                <template #default="props">
                    <div m="4">
                        <p m="t-0 b-2">商家信息</p>
                        <p m="t-0 b-2">地址：{{ props.row.merchantAddress }}</p>
                        <p m="t-0 b-2">营业时间：{{formatTime(props.row.timeforOpenBusiness)}}-{{ formatTime(props.row.timeforCloseBusiness) }}</p>
                        <p m="t-0 b-2">联系电话：{{ props.row.contact }}</p>
                        <p m="t-0 b-2">是否可以使用通用优惠券：{{ props.row.couponType ? '否' : '是' }}</p>
                        <p m="t-0 b-2">商家评分：{{ props.row.avgRating }}</p>
                        <button @click="deleteMerchant(currentMerchantId)">销号</button>
                
                    </div>
                </template>

            </el-table-column>
            <el-table-column label="商家" prop="merchantName" />
            <el-table-column label="商品类别" prop="dishType" />
            </el-table>
    </div>


</template>
<style scoped>


</style>

