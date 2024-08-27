<script setup>
import { ElMessage } from "element-plus";
import { ref, onMounted, computed} from 'vue';  
import { useRouter } from 'vue-router';
import {getCouponIds,getCouponsInfo,editCouponsInfo,createCoupon} from '@/api/platform';
const router = useRouter();
const couponIds= ref([]);  // 优惠券id列表
const couponsInfo = ref([]);  // 优惠券信息列表
const notOnShelvesCouponsInfo = ref([]);  // 未上架优惠券信息列表
const isOnShelvesCouponsInfo = ref([]);  // 上架优惠券信息列表
const showCouponsInfo = ref([]);  // 显示优惠券信息列表
const searchQuery = ref('');  // 搜索框内容
const isOnShelves = ref(true);  // 优惠券上下架状态
const isCouponInfo= ref(false);  // 优惠券详情状态
const currentCoupon=ref({
    CouponId: 0,  
    CouponName: '',  
    CouponValue: '',  
    CouponPrice:  '',  
    CouponType: 0,  
    MinPrice:  '',  
    PeriodOfValidity: '',  
    QuantitySold:  0,  
    IsOnShelves: 1,  
});  // 当前优惠券信息
const isEditing= ref(false);  // 优惠券编辑状态
const beforeUpdateIsOnShelves = ref(1); 
const refForm = ref(null); // 创建一个 ref 来引用 el-form
const isCouponCreate= ref(false);  // 优惠券创建状态
onMounted(() => {
    getCouponIds().then(res => {  // 获取所有商家id
        couponIds.value = res.data;  
        return Promise.all(couponIds.value.map(id => getCouponsInfo(id))); // 并发请求所有商家信息
    }).then(responses => {  
        couponsInfo.value = responses.map(response => response.data); // 提取商家信息  
        notOnShelvesCouponsInfo.value = couponsInfo.value.filter(coupon => coupon.isOnShelves === 0);  // 筛选未上架优惠券信息
        isOnShelvesCouponsInfo.value = couponsInfo.value.filter(coupon => coupon.isOnShelves === 1);  // 筛选上架优惠券信息
        showCouponsInfo.value = isOnShelvesCouponsInfo.value;  // 显示上架优惠券信息
        sortCouponsBySales();  // 按销量排序
        console.log(showCouponsInfo.value);
    }).catch(err => {  
        ElMessage.error('获取优惠券id失败'); 
    }); 
});
const sortCouponsBySales = () => {  
    showCouponsInfo.value.sort((a, b) => {  
        return b.quantitySold - a.quantitySold; // 降序排列  
    });  
};  
const couponRules = computed(() => {  
    const rules = {  
        CouponName: [  
            { required: true, message: '请输入优惠券名称', trigger: 'blur' },  
        ],  
        CouponValue: [  
            { required: true, message: '请输入优惠券抵扣金额', trigger: 'blur' }, 
            {  
                pattern: /^\d+(\.\d+)?$/, // 允许的格式：整数或一位小数  
                message: '抵扣金额必须是整数或小数', // 错误提示消息  
                trigger: 'change',  
            },  
        ],  
        CouponPrice: [  
            { required: true, message: '请输入优惠券价格', trigger: 'blur' }, 
            {  
                pattern: /^\d+(\.\d+)?$/, // 允许的格式：整数或一位小数  
                message: '价格必须是整数或小数', // 错误提示消息  
                trigger: 'change',  
            },  
        ],  
        MinPrice: [  
            { required: true, message: '请输入优惠券使用门槛', trigger: 'blur' }, 
            {  
                pattern: /^\d+(\.\d+)?$/, // 允许的格式：整数或一位小数  
                message: '使用门槛必须是整数或小数', // 错误提示消息  
                trigger: 'change',  
            },  
        ],  
        PeriodOfValidity: [  
            { required: true, message: '请输入优惠券有效天数', trigger: 'blur' }, 
            {
                pattern: /^[0-9]*$/, // 正则表达式，确保输入是数字  
                message: '有效天数只能是数字', // 错误提示消息  
                trigger: 'change',  
            },
        ],  
    };  
    return rules; // 返回动态生成的规则  
});  
const validateField = (field) => {  //编辑规则的应用
    refForm.value.validateField(field, (valid) => {  
        if (!valid) {  
            console.log(`验证失败: ${field}`); // 可以根据需要修改  
        }  
    });  
};  
const gobackHome = () => {
    router.push('/platform-home');
}
const handleSearch = () => {   //字符串匹配搜索商家
    const query = searchQuery.value.trim();  
    if(isOnShelves.value){  
        if (query) {  
            showCouponsInfo.value = isOnShelvesCouponsInfo.value.filter(coupon =>  
                String(coupon.couponId).includes(query) ||  
                coupon.couponName.includes(query)
            );  
        } else {  
            showCouponsInfo.value = isOnShelvesCouponsInfo.value;  
        }
    }
    else{  
        if (query) {  
            showCouponsInfo.value = notOnShelvesCouponsInfo.value.filter(coupon =>  
                String(coupon.couponId).includes(query) ||  
                coupon.couponName.includes(query)
            );  
        } else {  
            showCouponsInfo.value = notOnShelvesCouponsInfo.value;  
        }
    }
};  
const updateCoupon = () => {  
    const id=currentCoupon.value.couponId;  
    const isOnShelves=currentCoupon.value.isOnShelves;  
    editCouponsInfo(id,isOnShelves).then(res => {  
        ElMessage.success('修改成功');  
        isEditing.value = false;  
        getCouponIds().then(res => {  // 获取所有id
            couponIds.value = res.data;  
            return Promise.all(couponIds.value.map(id => getCouponsInfo(id))); // 并发请求所有信息
        }).then(responses => {  
            couponsInfo.value = responses.map(response => response.data); // 提取信息  
            notOnShelvesCouponsInfo.value = couponsInfo.value.filter(coupon => coupon.isOnShelves === 0);  // 筛选未上架优惠券信息
            isOnShelvesCouponsInfo.value = couponsInfo.value.filter(coupon => coupon.isOnShelves === 1);  // 筛选上架优惠券信息
            showCouponsInfo.value = isOnShelvesCouponsInfo.value;  
            console.log(showCouponsInfo.value);
        }).catch(err => {  
            ElMessage.error('获取优惠券id失败'); 
        }); 
    }).catch(err => {  
        ElMessage.error('修改失败');  
    });  
};
const submitCouponCreate = async () => {  
    const isValid = await refForm.value.validate(); 
    if (!isValid) return; 
    console.log(currentCoupon.value);
    createCoupon(currentCoupon.value).then(res => {  
        ElMessage.success('发布成功');  
        isCouponCreate.value = false;  
        getCouponIds().then(res => {  // 获取所有id
            couponIds.value = res.data;  
            return Promise.all(couponIds.value.map(id => getCouponsInfo(id))); // 并发请求所有信息
        }).then(responses => {  
            couponsInfo.value = responses.map(response => response.data); // 提取信息  
            notOnShelvesCouponsInfo.value = couponsInfo.value.filter(coupon => coupon.isOnShelves === 0);  // 筛选未上架优惠券信息
            isOnShelvesCouponsInfo.value = couponsInfo.value.filter(coupon => coupon.isOnShelves === 1);  // 筛选上架优惠券信息
            showCouponsInfo.value = isOnShelvesCouponsInfo.value;  
            console.log(showCouponsInfo.value);
        }).catch(err => {  
            ElMessage.error('获取优惠券id失败'); 
        }); 
    }).catch(err => {  
        ElMessage.error('发布失败');  
    });  
}
const showNotOnShelves = () => {  
    isOnShelves.value =false;  
    showCouponsInfo.value = notOnShelvesCouponsInfo.value;
};  
const showIsOnShelves = () => {  
    isOnShelves.value = true;  
    showCouponsInfo.value = isOnShelvesCouponsInfo.value;
};  
const enterCouponInfo = (coupon) => {  
    currentCoupon.value = coupon;
    isCouponInfo.value = true;  
};  
const leaveCouponInfo = () => {  
    isCouponInfo.value = false;  
    currentCoupon.value = {};  
};  
const editCoupon = () => {  
    beforeUpdateIsOnShelves.value = currentCoupon.value.isOnShelves;  
    isEditing.value = true;  
};  
const cancelEditCoupon = () => {  
    currentCoupon.value.isOnShelves = beforeUpdateIsOnShelves.value;  
    isEditing.value = false;  
};  
const enterCouponCreate = () => {  
    isCouponCreate.value = true;  
};  
const leaveCouponCreate = () => {  
    isCouponCreate.value = false;  
    currentCoupon.value = {  
        CouponId: 0,  
        CouponName: '',  
        CouponValue: '',  
        CouponPrice:  '',  
        CouponType: 0,  
        MinPrice:  '',  
        PeriodOfValidity: '',  
        QuantitySold:  0,  
        IsOnShelves: 1,  
    };  
};  
</script>

<template>
    <div v-if="!isCouponInfo&&!isCouponCreate">
        优惠券管理
        <button @click="gobackHome">返回</button>
        <h2>优惠券列表</h2>
        <labal @click="showIsOnShelves">上架中</labal>&nbsp;&nbsp;
        <labal @click="showNotOnShelves">已下架</labal>
        <div>
            <input type="text" v-model="searchQuery" placeholder="搜索Id或名称" v-on:keyup.enter="handleSearch()"/> 
            <button @click="handleSearch()">搜索</button>
            <button @click="enterCouponCreate()">发布优惠券</button>
        </div>
        <ul>  
            <li v-for="coupon in showCouponsInfo" :key="coupon.couponId">  
                <span>{{ coupon.couponId }}</span> 
                <span>&nbsp;&nbsp;{{ coupon.couponName }}</span> 
                <span>&nbsp;&nbsp;销量:{{ coupon.quantitySold }}</span> 
                <span>&nbsp;<button @click="enterCouponInfo(coupon)">></button></span>
            </li>  
        </ul> 
    </div>

    <div v-if="isCouponInfo&&!isCouponCreate">
        <h2>优惠券详情</h2>
        <div>
            <p>优惠券Id:&nbsp;&nbsp;&nbsp;{{ currentCoupon.couponId }}</p>
            <p>优惠券名称:&nbsp;&nbsp;&nbsp;{{ currentCoupon.couponName }}</p>
            <p>优惠券抵扣金额:&nbsp;&nbsp;&nbsp;{{ currentCoupon.couponValue }}元</p>
            <p>优惠券价格:&nbsp;&nbsp;&nbsp;{{ currentCoupon.couponPrice }}元</p>
            <p>优惠券类型:&nbsp;&nbsp;&nbsp;{{ currentCoupon.couponType===0?'通用优惠券':'特殊优惠券' }}</p>
            <p>优惠券使用门槛:&nbsp;&nbsp;&nbsp;{{ currentCoupon.minPrice }}</p>
            <p>优惠券有效天数:&nbsp;&nbsp;&nbsp;{{ currentCoupon.periodOfValidity }}天</p>
            <p>优惠券销量:&nbsp;&nbsp;&nbsp;{{ currentCoupon.quantitySold }}</p>
            <p>优惠券上架状态:&nbsp;&nbsp;&nbsp;{{ currentCoupon.isOnShelves === 1 ? '上架中' : '已下架' }}</p>
        </div>
        <div v-if="isEditing">  
                <label>  
                    <input type="radio" v-model="currentCoupon.isOnShelves" :value="1" /> 上架  
                </label>  
                <label>  
                    <input type="radio" v-model="currentCoupon.isOnShelves" :value="0" /> 下架  
                </label>  
            </div>  
        <div>
            <button @click="leaveCouponInfo()">返回</button>
            <button v-if="!isEditing" @click="editCoupon()">编辑</button>
            <button v-if="isEditing" @click="updateCoupon()">保存</button>
            <button v-if="isEditing" @click="cancelEditCoupon()">取消编辑</button>
        </div>
    </div>
    <div v-if="isCouponCreate">
        <h2>创建优惠券</h2>
        <el-form ref="refForm" :rules="couponRules" :model="currentCoupon">  
        <el-form-item label="优惠券名称" prop="CouponName">  
            <input v-model="currentCoupon.CouponName" placeholder="请输入优惠券名称" @blur="validateField('CouponName')" />  
        </el-form-item>  
        <el-form-item label="优惠券抵扣金额" prop="CouponValue">  
            <input v-model="currentCoupon.CouponValue" placeholder="请输入优惠券抵扣金额" @blur="validateField('CouponValue')" />  
        </el-form-item>  
        <el-form-item label="优惠券价格" prop="CouponPrice">  
            <input v-model="currentCoupon.CouponPrice" placeholder="请输入优惠券价格" @blur="validateField('CouponPrice')" />  
        </el-form-item>  
        <el-form-item label="优惠券类型" prop="CouponType">  
            <el-radio-group v-model="currentCoupon.CouponType">  
                <el-radio label="0">通用优惠券</el-radio>  
                <el-radio label="1">特殊优惠券</el-radio>  
            </el-radio-group>  
        </el-form-item>  
        <el-form-item label="优惠券使用门槛" prop="MinPrice">  
            <input v-model="currentCoupon.MinPrice" placeholder="请输入优惠券使用门槛" @blur="validateField('MinPrice')" />  
        </el-form-item>  
        <el-form-item label="优惠券有效天数" prop="PeriodOfValidity">  
            <input type="number" v-model="currentCoupon.PeriodOfValidity" placeholder="请输入优惠券有效天数" @blur="validateField('PeriodOfValidity')" />  
        </el-form-item>  
        </el-form>  
        <button @click="submitCouponCreate()">发布优惠券</button>&nbsp;&nbsp;
        <button @click="leaveCouponCreate()">取消</button>
    </div>

</template>